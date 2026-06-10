#!/usr/bin/env python3
"""
Генератор тестовых транзакций для проекта SmartWallet.

Генерирует реалистичные данные транзакций расходов (Expense, Type=1)
за указанный период и загружает их в базу данных PostgreSQL.

Категории расходов и хранилища загружаются из базы данных,
диапазоны сумм и веса настраиваются через categories_config.json.

Целевая версия Python: 3.9+
"""

from __future__ import annotations

import argparse
import json
import math
import os
import random
import sys
import uuid
from dataclasses import dataclass, field
from datetime import date, datetime, time, timedelta
from typing import Any, Dict, List, Optional, Tuple

try:
    from zoneinfo import ZoneInfo
except Exception:
    # On Windows, zoneinfo requires the tzdata package
    sys.exit(
        "Ошибка: не удалось загрузить модуль zoneinfo. "
        "Установите пакет tzdata: pip install tzdata"
    )

try:
    from dotenv import load_dotenv
except ImportError:
    sys.exit(
        "Ошибка: модуль python-dotenv не установлен. "
        "Выполните: pip install -r requirements.txt"
    )

try:
    import psycopg2
    from psycopg2.extras import execute_values
except ImportError:
    psycopg2 = None  # Будет ошибка только при попытке подключения к БД


# ============================================================
# Константы
# ============================================================

try:
    ЧАСОВОЙ_ПОЯС = ZoneInfo("Europe/Moscow")
except Exception:
    # On Windows, ZoneInfo instantiation fails without the tzdata package
    sys.exit(
        "Ошибка: не удалось загрузить часовой пояс Europe/Moscow. "
        "Установите пакет tzdata: pip install tzdata"
    )

# Начало и конец рабочего дня для генерации транзакций
ЧАС_НАЧАЛО = 8
ЧАС_КОНЕЦ = 23

# Имя файла конфигурации категорий
КАТЕГОРИИ_CONFIG_FILE = "categories_config.json"


class TransactionType:
    """Типы транзакций (соответствуют C# enum TransactionType)."""

    TRANSFER = 0
    EXPENSE = 1
    ADJUSTMENT_DECREASE = 2
    ADJUSTMENT_INCREASE = 3
    INCOME = 4
    FOR_TEST = 5


ТИПЫ_НАЗВАНИЯ: Dict[int, str] = {
    TransactionType.TRANSFER: "Transfer (перевод)",
    TransactionType.EXPENSE: "Expense (трата)",
    TransactionType.ADJUSTMENT_DECREASE: "AdjustmentDecrease (корректировка -)",
    TransactionType.ADJUSTMENT_INCREASE: "AdjustmentIncrease (корректировка +)",
    TransactionType.INCOME: "Income (доход)",
    TransactionType.FOR_TEST: "ForTest",
}

# SQL-запрос для вставки одной транзакции
INSERT_SQL = """\
INSERT INTO "Transaction"
    ("Id", "UserId", "Amount", "MadeAt", "DeletedAt",
     "DestinationAccountId", "SourceAccountId", "Type")
VALUES %s
"""

# SQL-запрос для загрузки конечных точек транзакций из БД
FETCH_ENDPOINTS_SQL = """\
SELECT "Id", "Name", "IsStorage", "Value"
FROM "TransactionEndpoint"
WHERE "UserId" = %s AND "DeletedAt" IS NULL
"""


# ============================================================
# Конфигурация категорий (из JSON)
# ============================================================

def load_categories_config(script_dir: str) -> Dict[str, Any]:
    """
    Загружает конфигурацию категорий из файла categories_config.json.

    Возвращает словарь с настройками: диапазоны сумм, веса, значения по умолчанию.
    """
    config_path = os.path.join(script_dir, КАТЕГОРИИ_CONFIG_FILE)
    if not os.path.isfile(config_path):
        sys.exit(
            f"Ошибка: файл конфигурации категорий не найден:\n"
            f"  {config_path}\n"
            f"Создайте файл {КАТЕГОРИИ_CONFIG_FILE} в директории scripts/."
        )

    try:
        with open(config_path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except json.JSONDecodeError as e:
        sys.exit(f"Ошибка парсинга JSON в {config_path}:\n  {e}")
    except OSError as e:
        sys.exit(f"Ошибка чтения файла {config_path}:\n  {e}")

    # Валидация обязательных полей
    required_keys = ["default_expense_range", "default_expense_weight", "categories"]
    for key in required_keys:
        if key not in data:
            sys.exit(f"Ошибка: обязательное поле '{key}' отсутствует в {config_path}.")

    return data


# ============================================================
# Конфигурация
# ============================================================

@dataclass(frozen=True)
class CategoryPatternConfig:
    """Паттерн генерации для одной категории расходов."""

    min_amount: float
    max_amount: float
    weight: int
    seasonal_amplitude: float
    seasonal_period_days: int
    seasonal_phase_day: int
    weekday_multiplier: float
    weekend_multiplier: float
    anchor_day: Optional[int] = None
    anchor_amount: Optional[float] = None


@dataclass(frozen=True)
class Config:
    """Конфигурация генератора, загружаемая из .env и базы данных."""

    user_id: str
    days_back: int
    tx_per_day: int
    db_connection_string: str

    # Данные, загруженные из БД
    storage_ids: List[str] = field(default_factory=list)
    expense_category_ids: List[str] = field(default_factory=list)
    # Отображение UUID -> название категории (для отчётов)
    category_names: Dict[str, str] = field(default_factory=dict)
    # Диапазоны сумм по UUID категории: {UUID: (мин, макс)}
    category_ranges: Dict[str, Tuple[float, float]] = field(default_factory=dict)
    # Веса вероятности по UUID категории: {UUID: weight}
    category_weights: Dict[str, int] = field(default_factory=dict)
    # Паттерны генерации по UUID категории: {UUID: CategoryPatternConfig}
    category_patterns: Dict[str, CategoryPatternConfig] = field(default_factory=dict)
    # День зарплаты (информационное поле, зарезервировано для Income)
    salary_day: int = 5
    # Сумма зарплаты (информационное поле)
    salary_amount: float = 80000.0
    # Диапазон случайного шума для итоговой суммы
    noise_range: Tuple[float, float] = (0.85, 1.15)


def load_config() -> Config:
    """
    Загружает конфигурацию из .env файла и базы данных.

    1. Читает базовые параметры из .env (USER_ID, DAYS_BACK и т.д.)
    2. Подключается к БД и загружает конечные точки транзакций
    3. Сопоставляет категории из БД с настройками из categories_config.json
    4. Формирует итоговую конфигурацию
    """
    # Загружаем .env из директории скрипта
    script_dir = os.path.dirname(os.path.abspath(__file__))
    env_path = os.path.join(script_dir, ".env")
    load_dotenv(env_path)

    user_id = os.getenv("USER_ID", "")
    if not user_id:
        sys.exit("Ошибка: USER_ID не задан в .env файле.")

    days_back = int(os.getenv("DAYS_BACK", "0"))
    tx_per_day = int(os.getenv("TRANSACTIONS_PER_DAY", "12"))

    db_conn = os.getenv("DB_CONNECTION_STRING", "")
    if not db_conn:
        sys.exit("Ошибка: DB_CONNECTION_STRING не задан в .env файле.")

    # Загружаем конфигурацию категорий из JSON
    categories_config = load_categories_config(script_dir)

    # Подключаемся к БД для загрузки конечных точек транзакций
    print("  Загрузка категорий из базы данных...")
    conn = _connect_for_config(db_conn)
    try:
        endpoints = _fetch_endpoints(conn, user_id)
    finally:
        conn.close()

    if not endpoints:
        sys.exit(
            f"Ошибка: для пользователя {user_id} не найдено ни одной "
            f"конечной точки транзакций в базе данных."
        )

    # Разделяем на хранилища и категории расходов
    storage_ids: List[str] = []
    expense_category_ids: List[str] = []
    category_names: Dict[str, str] = {}

    for ep_id, ep_name, is_storage, _value in endpoints:
        if is_storage:
            storage_ids.append(ep_id)
        else:
            expense_category_ids.append(ep_id)
            category_names[ep_id] = ep_name

    if not storage_ids:
        sys.exit(
            "Ошибка: для пользователя не найдено ни одного хранилища "
            "(TransactionEndpoint с IsStorage=True)."
        )

    if not expense_category_ids:
        sys.exit(
            "Ошибка: для пользователя не найдено ни одной категории расходов "
            "(TransactionEndpoint с IsStorage=False)."
        )

    # Сопоставляем категории с конфигурацией из JSON
    json_categories = categories_config.get("categories", {})
    default_range = tuple(categories_config["default_expense_range"])
    default_weight = int(categories_config["default_expense_weight"])

    # Значения по умолчанию для паттернов генерации
    def_seasonal_amplitude = float(
        categories_config.get("default_seasonal_amplitude", 0.2)
    )
    def_seasonal_period = int(
        categories_config.get("default_seasonal_period_days", 30)
    )
    def_seasonal_phase = int(
        categories_config.get("default_seasonal_phase_day", 1)
    )
    def_weekday_mult = float(
        categories_config.get("default_weekday_multiplier", 1.0)
    )
    def_weekend_mult = float(
        categories_config.get("default_weekend_multiplier", 1.0)
    )

    # Глобальные параметры генерации
    salary_day = int(categories_config.get("salary_day", 5))
    salary_amount = float(categories_config.get("salary_amount", 80000.0))
    noise_range_list = categories_config.get("noise_range", [0.85, 1.15])
    noise_range_cfg = (float(noise_range_list[0]), float(noise_range_list[1]))

    category_ranges: Dict[str, Tuple[float, float]] = {}
    category_weights: Dict[str, int] = {}
    category_patterns: Dict[str, CategoryPatternConfig] = {}

    for ep_id, ep_name, is_storage, _value in endpoints:
        if is_storage:
            continue

        # Ищем категорию по имени в JSON-конфигурации
        cat_config = json_categories.get(ep_name)
        if cat_config:
            category_ranges[ep_id] = (
                float(cat_config.get("min", default_range[0])),
                float(cat_config.get("max", default_range[1])),
            )
            category_weights[ep_id] = int(cat_config.get("weight", default_weight))
            category_patterns[ep_id] = CategoryPatternConfig(
                min_amount=float(cat_config.get("min", default_range[0])),
                max_amount=float(cat_config.get("max", default_range[1])),
                weight=int(cat_config.get("weight", default_weight)),
                seasonal_amplitude=float(
                    cat_config.get("seasonal_amplitude", def_seasonal_amplitude)
                ),
                seasonal_period_days=int(
                    cat_config.get("seasonal_period_days", def_seasonal_period)
                ),
                seasonal_phase_day=int(
                    cat_config.get("seasonal_phase_day", def_seasonal_phase)
                ),
                weekday_multiplier=float(
                    cat_config.get("weekday_multiplier", def_weekday_mult)
                ),
                weekend_multiplier=float(
                    cat_config.get("weekend_multiplier", def_weekend_mult)
                ),
                anchor_day=(
                    int(cat_config["anchor_day"])
                    if "anchor_day" in cat_config
                    else None
                ),
                anchor_amount=(
                    float(cat_config["anchor_amount"])
                    if "anchor_amount" in cat_config
                    else None
                ),
            )
        else:
            # Категория не найдена в JSON — используем значения по умолчанию
            category_ranges[ep_id] = default_range
            category_weights[ep_id] = default_weight
            category_patterns[ep_id] = CategoryPatternConfig(
                min_amount=default_range[0],
                max_amount=default_range[1],
                weight=default_weight,
                seasonal_amplitude=def_seasonal_amplitude,
                seasonal_period_days=def_seasonal_period,
                seasonal_phase_day=def_seasonal_phase,
                weekday_multiplier=def_weekday_mult,
                weekend_multiplier=def_weekend_mult,
            )
            print(
                f"  [!] Категория '{ep_name}' не найдена в {КАТЕГОРИИ_CONFIG_FILE}, "
                f"используются значения по умолчанию."
            )

    return Config(
        user_id=user_id,
        days_back=days_back,
        tx_per_day=tx_per_day,
        db_connection_string=db_conn,
        storage_ids=storage_ids,
        expense_category_ids=expense_category_ids,
        category_names=category_names,
        category_ranges=category_ranges,
        category_weights=category_weights,
        category_patterns=category_patterns,
        salary_day=salary_day,
        salary_amount=salary_amount,
        noise_range=noise_range_cfg,
    )


def _connect_for_config(db_connection_string: str):
    """
    Устанавливает подключение к PostgreSQL для загрузки конфигурации.

    Возвращает объект подключения psycopg2.
    """
    if psycopg2 is None:
        sys.exit(
            "Ошибка: модуль psycopg2 не установлен. "
            "Выполните: pip install -r requirements.txt"
        )

    conn_str = convert_dotnet_connection_string(db_connection_string)

    try:
        conn = psycopg2.connect(conn_str)
        conn.autocommit = True
        return conn
    except psycopg2.OperationalError as e:
        sys.exit(f"Ошибка подключения к базе данных:\n  {e}")
    except Exception as e:
        sys.exit(f"Неожиданная ошибка подключения:\n  {e}")


def _fetch_endpoints(conn, user_id: str) -> List[Tuple[str, str, bool, float]]:
    """
    Загружает конечные точки транзакций пользователя из БД.

    Возвращает список кортежей (Id, Name, IsStorage, Value).
    """
    cur = conn.cursor()
    try:
        cur.execute(FETCH_ENDPOINTS_SQL, (user_id,))
        rows = cur.fetchall()
        # Приводим UUID к строковому формату
        return [(str(row[0]), row[1], row[2], float(row[3])) for row in rows]
    except Exception as e:
        sys.exit(f"Ошибка загрузки конечных точек транзакций:\n  {e}")
    finally:
        cur.close()


def convert_dotnet_connection_string(dotnet_cs: str) -> str:
    """
    Конвертирует строку подключения из формата .NET/Npgsql в формат libpq
    для psycopg2.

    Пример:
        Host=localhost;Port=5432;Database=db;Username=user;Password=pass
        -> host=localhost port=5432 dbname=db user=user password=pass
    """
    # Если строка уже в формате libpq (нет точек с запятой как разделителей)
    if ";" not in dotnet_cs:
        return dotnet_cs

    # Сопоставление ключей .NET -> libpq
    key_map = {
        "host": "host",
        "server": "host",
        "data source": "host",
        "port": "port",
        "database": "dbname",
        "db": "dbname",
        "initial catalog": "dbname",
        "username": "user",
        "user": "user",
        "user id": "user",
        "password": "password",
        "pwd": "password",
        "ssl mode": "sslmode",
        "trust server certificate": "sslmode",
    }

    parts: List[str] = []
    for segment in dotnet_cs.split(";"):
        segment = segment.strip()
        if not segment or "=" not in segment:
            continue
        key, value = segment.split("=", 1)
        key_lower = key.strip().lower()
        libpq_key = key_map.get(key_lower)
        if libpq_key:
            # Специальная обработка для Trust Server Certificate
            if key_lower == "trust server certificate":
                if value.strip().lower() in ("true", "1", "yes"):
                    parts.append("sslmode=require")
                continue
            parts.append(f"{libpq_key}={value.strip()}")

    return " ".join(parts)


# ============================================================
# Генерация транзакций
# ============================================================

def _random_time_in_day() -> time:
    """Генерирует случайное время в диапазоне 8:00 — 23:00."""
    hour = random.randint(ЧАС_НАЧАЛО, ЧАС_КОНЕЦ - 1)
    minute = random.randint(0, 59)
    second = random.randint(0, 59)
    return time(hour, minute, second)


def _make_timestamp(day: date) -> datetime:
    """Создаёт timestamp с часовым поясом Europe/Moscow для указанного дня."""
    return datetime.combine(day, _random_time_in_day(), tzinfo=ЧАСОВОЙ_ПОЯС)


def _compute_seasonal_multiplier(
    day: date,
    pattern: CategoryPatternConfig,
) -> float:
    """
    Вычисляет сезонный множитель для данного дня и категории.

    Формула: 1 + amplitude * sin(2 * pi * (day_of_month - phase) / period)
    Возвращает значение в диапазоне [1 - amplitude, 1 + amplitude].
    """
    day_of_month = day.day
    return 1.0 + pattern.seasonal_amplitude * math.sin(
        2 * math.pi * (day_of_month - pattern.seasonal_phase_day)
        / pattern.seasonal_period_days
    )


def _compute_weekday_multiplier(
    day: date,
    pattern: CategoryPatternConfig,
) -> float:
    """
    Возвращает множитель дня недели для категории.

    Выходные (Saturday=5, Sunday=6) -> weekend_multiplier.
    Будние (Monday=0 .. Friday=4)   -> weekday_multiplier.
    """
    if day.weekday() >= 5:
        return pattern.weekend_multiplier
    return pattern.weekday_multiplier


def _compute_transaction_probability(
    day: date,
    pattern: CategoryPatternConfig,
    tx_per_day: int,
) -> float:
    """
    Вычисляет вероятность генерации транзакции категории в данный день.

    Базовая вероятность = weight / tx_per_day.
    Для выходных корректируется отношением weekend/weekday.
    Результат ограничивается диапазоном [0.0, 1.0].
    """
    base_prob = pattern.weight / tx_per_day
    if day.weekday() >= 5:
        ratio = pattern.weekend_multiplier / pattern.weekday_multiplier
        base_prob *= ratio
    return min(base_prob, 1.0)


def _compute_amount(
    day: date,
    pattern: CategoryPatternConfig,
    noise_range: Tuple[float, float],
) -> float:
    """
    Вычисляет итоговую сумму транзакции по гибридной модели.

    final = base * seasonal * weekday * noise
    """
    base = random.uniform(pattern.min_amount, pattern.max_amount)
    seasonal = _compute_seasonal_multiplier(day, pattern)
    weekday = _compute_weekday_multiplier(day, pattern)
    noise = random.uniform(noise_range[0], noise_range[1])
    return round(base * seasonal * weekday * noise, 2)





def generate_expense(
    config: Config,
    day: date,
    category_id: str,
    amount: float,
) -> Dict[str, Any]:
    """
    Создаёт транзакцию расхода (Type=1) для указанной категории и суммы.

    Функция является чистым конструктором транзакции:
    категория и сумма вычисляются вызывающим кодом.
    """
    source_id = random.choice(config.storage_ids)

    return {
        "id": str(uuid.uuid4()),
        "user_id": config.user_id,
        "amount": amount,
        "made_at": _make_timestamp(day),
        "deleted_at": None,
        "source_account_id": source_id,
        "destination_account_id": category_id,
        "type": TransactionType.EXPENSE,
    }


def generate_all_transactions(config: Config) -> List[Dict[str, Any]]:
    """
    Генерирует все транзакции расходов за период [today - days_back, today].

    Алгоритм: для каждого дня перебирает все категории расходов и независимо
    решает, сгенерировать ли транзакцию для этой категории в этот день.
    Вероятность и сумма определяются паттерном категории (сезонность,
    день недели, якорные события, шум).
    """
    tz = ЧАСОВОЙ_ПОЯС
    today = datetime.now(tz).date()
    start_date = today - timedelta(days=config.days_back)

    all_transactions: List[Dict[str, Any]] = []

    current_date = start_date
    while current_date <= today:
        for cat_id in config.expense_category_ids:
            pattern = config.category_patterns.get(cat_id)
            if pattern is None:
                continue

            # --- Якорная транзакция ---
            if (
                pattern.anchor_day is not None
                and pattern.anchor_amount is not None
                and current_date.day == pattern.anchor_day
            ):
                tx = generate_expense(
                    config, current_date, cat_id, pattern.anchor_amount
                )
                all_transactions.append(tx)
                continue

            # --- Вероятность транзакции в этот день ---
            prob = _compute_transaction_probability(
                current_date, pattern, config.tx_per_day
            )
            if random.random() >= prob:
                continue

            # --- Расчёт суммы ---
            amount = _compute_amount(
                current_date, pattern, config.noise_range
            )
            tx = generate_expense(config, current_date, cat_id, amount)
            all_transactions.append(tx)

        current_date += timedelta(days=1)

    # Сортируем по времени для хронологического порядка
    all_transactions.sort(key=lambda tx: tx["made_at"])

    return all_transactions


# ============================================================
# Работа с базой данных
# ============================================================

def connect_db(config: Config):
    """
    Устанавливает подключение к PostgreSQL для вставки данных.

    Возвращает объект подключения psycopg2.
    """
    if psycopg2 is None:
        sys.exit(
            "Ошибка: модуль psycopg2 не установлен. "
            "Выполните: pip install -r requirements.txt"
        )

    conn_str = convert_dotnet_connection_string(config.db_connection_string)

    try:
        conn = psycopg2.connect(conn_str)
        conn.autocommit = False
        return conn
    except psycopg2.OperationalError as e:
        sys.exit(f"Ошибка подключения к базе данных:\n  {e}")
    except Exception as e:
        sys.exit(f"Неожиданная ошибка подключения:\n  {e}")


def clear_user_transactions(conn, user_id: str, dry_run: bool = False) -> int:
    """
    Удаляет все транзакции пользователя.

    Возвращает количество удалённых записей.
    """
    delete_sql = 'DELETE FROM "Transaction" WHERE "UserId" = %s'

    if dry_run:
        print(f"  [DRY RUN] {delete_sql}")
        print(f"  [DRY RUN] Параметры: user_id={user_id}")
        return 0

    cur = conn.cursor()
    try:
        cur.execute(delete_sql, (user_id,))
        deleted_count = cur.rowcount
        conn.commit()
        return deleted_count
    except Exception as e:
        conn.rollback()
        sys.exit(f"Ошибка при удалении транзакций:\n  {e}")
    finally:
        cur.close()


def insert_transactions(
    conn,
    transactions: List[Dict[str, Any]],
    dry_run: bool = False,
    batch_size: int = 500,
) -> int:
    """
    Вставляет транзакции в базу данных пакетно.

    Возвращает количество вставленных записей.
    """
    if not transactions:
        print("  Нет транзакций для вставки.")
        return 0

    if dry_run:
        # Показываем первые 3 и последние 3 записи
        shown = min(3, len(transactions))
        print(f"  [DRY RUN] Примеры INSERT (первые {shown} из {len(transactions)}):")
        for tx in transactions[:shown]:
            values = _tx_to_params(tx)
            print(f"    {INSERT_SQL.strip()}")
            print(f"    -- VALUES {values}")
            print()
        if len(transactions) > shown * 2:
            print(f"  ... ещё {len(transactions) - shown * 2} записей ...")
            print()
            print(f"  [DRY RUN] Последние {shown}:")
            for tx in transactions[-shown:]:
                values = _tx_to_params(tx)
                print(f"    {INSERT_SQL.strip()}")
                print(f"    -- VALUES {values}")
                print()
        elif len(transactions) > shown:
            print(f"  [DRY RUN] Оставшиеся:")
            for tx in transactions[shown:]:
                values = _tx_to_params(tx)
                print(f"    {INSERT_SQL.strip()}")
                print(f"    -- VALUES {values}")
                print()
        return len(transactions)

    # Пакетная вставка через execute_values для производительности
    inserted = 0
    cur = conn.cursor()
    try:
        for i in range(0, len(transactions), batch_size):
            batch = transactions[i : i + batch_size]
            values_list = [_tx_to_params(tx) for tx in batch]

            execute_values(
                cur,
                INSERT_SQL,
                values_list,
                template=None,
                page_size=batch_size,
            )
            inserted += len(batch)

        conn.commit()
        return inserted

    except Exception as e:
        conn.rollback()
        sys.exit(f"Ошибка при вставке транзакций (вставлено {inserted}):\n  {e}")
    finally:
        cur.close()


def _tx_to_params(tx: Dict[str, Any]) -> tuple:
    """Преобразует словарь транзакции в кортеж параметров для SQL."""
    return (
        tx["id"],
        tx["user_id"],
        tx["amount"],
        tx["made_at"],
        tx["deleted_at"],
        tx["destination_account_id"],
        tx["source_account_id"],
        tx["type"],
    )


# ============================================================
# Вывод и отчёты
# ============================================================

def print_summary(transactions: List[Dict[str, Any]], config: Config) -> None:
    """Выводит сводку по сгенерированным транзакциям."""
    if not transactions:
        print("\nТранзакции не сгенерированы.")
        return

    # Подсчёт по типам
    by_type: Dict[int, List[Dict[str, Any]]] = {}
    for tx in transactions:
        by_type.setdefault(tx["type"], []).append(tx)

    # Диапазон дат
    dates = [tx["made_at"].date() for tx in transactions]
    min_date = min(dates)
    max_date = max(dates)
    total_days = (max_date - min_date).days + 1

    print("\n" + "=" * 60)
    print("  Сводка по сгенерированным транзакциям")
    print("=" * 60)
    print(f"  Период: {min_date.isoformat()} — {max_date.isoformat()} ({total_days} дн.)")
    print(f"  Всего транзакций: {len(transactions)}")
    print(f"  В среднем в день: {len(transactions) / max(total_days, 1):.1f}")
    print("-" * 60)
    print(f"  {'Тип':<35} {'Кол-во':>8} {'Сумма':>15}")
    print("-" * 60)

    total_amount = 0.0
    for type_id in sorted(by_type.keys()):
        txs = by_type[type_id]
        type_sum = sum(tx["amount"] for tx in txs)
        total_amount += type_sum
        name = ТИПЫ_НАЗВАНИЯ.get(type_id, f"Unknown ({type_id})")
        print(f"  {name:<35} {len(txs):>8} {type_sum:>15,.2f}")

    print("-" * 60)
    print(f"  {'ИТОГО':<35} {len(transactions):>8} {total_amount:>15,.2f}")
    print("=" * 60)

    # Топ категорий расходов
    expenses = by_type.get(TransactionType.EXPENSE, [])
    if expenses:
        category_stats: Dict[str, Tuple[int, float]] = {}
        for tx in expenses:
            cat_id = tx["destination_account_id"] or "N/A"
            count, amount = category_stats.get(cat_id, (0, 0.0))
            category_stats[cat_id] = (count + 1, amount + tx["amount"])

        print("\n  Топ категорий расходов:")
        print(f"  {'Категория':<25} {'Кол-во':>8} {'Сумма':>15}")
        print("  " + "-" * 50)
        sorted_cats = sorted(category_stats.items(), key=lambda x: x[1][1], reverse=True)
        for cat_id, (count, amount) in sorted_cats:
            # Используем названия из БД вместо захардкоженных
            name = config.category_names.get(cat_id, cat_id[:8] + "...")
            print(f"  {name:<25} {count:>8} {amount:>15,.2f}")
        print()


def print_config(config: Config) -> None:
    """Выводит текущую конфигурацию."""
    print("  Конфигурация:")
    print(f"    User ID:              {config.user_id}")
    print(f"    Хранилища:            {len(config.storage_ids)} шт.")
    print(f"    Категории расходов:   {len(config.expense_category_ids)} шт.")
    for cat_id in config.expense_category_ids:
        name = config.category_names.get(cat_id, cat_id[:8] + "...")
        pattern = config.category_patterns.get(cat_id)
        if pattern:
            rng_str = f"[{pattern.min_amount:,.0f}\u2013{pattern.max_amount:,.0f}]"
            parts = [
                f"вес={pattern.weight}",
                f"сезон={pattern.seasonal_amplitude}",
                f"пик={pattern.seasonal_phase_day}д",
                f"будни={pattern.weekday_multiplier}",
                f"вых={pattern.weekend_multiplier}",
            ]
            if pattern.anchor_day is not None:
                parts.append(
                    f"якорь={pattern.anchor_day}д/{pattern.anchor_amount:,.0f}"
                )
            info = " ".join(parts)
            print(f"      - {name:<25} {rng_str} {info}")
        else:
            rng = config.category_ranges.get(cat_id, (200, 2000))
            weight = config.category_weights.get(cat_id, 1)
            print(
                f"      - {name:<25} [{rng[0]:,.0f}\u2013{rng[1]:,.0f}] вес={weight}"
            )
    print(f"    Дней в прошлое:       {config.days_back}")
    print(f"    Транзакций/день:       ~{config.tx_per_day}")
    print(f"    Шум:                  [{config.noise_range[0]}, {config.noise_range[1]}]")
    print(f"    Зарплата:             день {config.salary_day}, {config.salary_amount:,.0f}")


# ============================================================
# CLI
# ============================================================

def parse_args() -> argparse.Namespace:
    """Парсит аргументы командной строки."""
    parser = argparse.ArgumentParser(
        description="Генератор тестовых транзакций для SmartWallet",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""\
Примеры:
  python seed_transactions.py                     # Генерация и вставка
  python seed_transactions.py --dry-run           # Только показать SQL
  python seed_transactions.py --clear             # Очистить и вставить
  python seed_transactions.py --clear --dry-run   # Показать DELETE + INSERT
""",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Вывести SQL-запросы без выполнения (подключение к БД только для загрузки категорий)",
    )
    parser.add_argument(
        "--clear",
        action="store_true",
        help="Удалить все существующие транзакции пользователя перед вставкой",
    )
    parser.add_argument(
        "--yes", "-y",
        action="store_true",
        help="Не запрашивать подтверждение при --clear",
    )
    return parser.parse_args()


# ============================================================
# Главная функция
# ============================================================

def main() -> None:
    """Точка входа в скрипт."""
    args = parse_args()

    print("\n  Генератор тестовых транзакций SmartWallet")
    print("  " + "-" * 50)

    # Загрузка конфигурации (включая подключение к БД для загрузки категорий)
    config = load_config()
    print("  Категории загружены.")
    print_config(config)

    # Генерация транзакций
    print("\n  Генерация транзакций...")
    transactions = generate_all_transactions(config)
    print(f"  Сгенерировано транзакций: {len(transactions)}")

    # Вывод сводки
    print_summary(transactions, config)

    # Режим dry-run — только выводим SQL
    if args.dry_run:
        print("\n  === DRY RUN MODE ===")
        print("  Примечание: подключение к БД использовалось только для загрузки категорий.")
        if args.clear:
            print("\n  Будет выполнено удаление:")
            clear_user_transactions(None, config.user_id, dry_run=True)
        print("\n  Будут вставлены следующие транзакции:")
        insert_transactions(None, transactions, dry_run=True)
        print("\n  [DRY RUN] Завершено. Данные не были изменены.")
        return

    # Подключение к БД и вставка
    print("\n  Подключение к базе данных...")
    conn = connect_db(config)
    print("  Подключение установлено.")

    try:
        # Очистка (если запрошена)
        if args.clear:
            if not args.yes:
                answer = input(
                    "\n  ВНИМАНИЕ: Все транзакции пользователя будут удалены. "
                    "Продолжить? (y/N): "
                )
                if answer.strip().lower() != "y":
                    print("  Отменено пользователем.")
                    return

            print("\n  Удаление существующих транзакций...")
            deleted = clear_user_transactions(conn, config.user_id)
            print(f"  Удалено транзакций: {deleted}")

        # Вставка
        print("\n  Вставка новых транзакций...")
        inserted = insert_transactions(conn, transactions)
        print(f"  Вставлено транзакций: {inserted}")

        print("\n  Готово!")

    except KeyboardInterrupt:
        print("\n\n  Прервано пользователем.")
        conn.rollback()
    finally:
        conn.close()
        print("  Подключение закрыто.")


if __name__ == "__main__":
    main()
