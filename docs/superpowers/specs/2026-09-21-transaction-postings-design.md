# Design: Transaction Postings API

| Поле | Значение |
|---|---|
| Статус | Согласовано |
| Дата | 2026-09-21 |
| Целевая версия | .NET 9 / C# 13 |
| БД | PostgreSQL, EF Core |
| Автор | SmartWallet Team |

---

## 1. Контекст и цели

Текущая модель создания транзакции (`CreateTransactionApiModel`) оперирует парой
`SourceAccountId / DestinationAccountId / Amount` и ограничена двумя конечными
точками. Это не покрывает сценарии:

- перевод между несколькими хранилищами за одну операцию;
- трата с нескольких карт с распределением по нескольким категориям;
- корректировка (adjustment) без указания пары.

Необходимо перейти к **postings-based** модели: клиент передаёт список
`Postings(accountId, amount)`, сервер классифицирует тип транзакции
автоматически на основе знаков сумм и типов счетов (Storage / Category / System).

### Цели

1. Гибкий API, принимающий N проводок в одной транзакции.
2. Серверная классификация типа транзакции по инвариантам проводок.
3. Скрытый System-счёт для балансировки Adjustment-операций.
4. Сохранение атомарности через существующий EF Core UnitOfWork.
5. Стабильные коды ошибок для клиентского парсинга.

### Не-цели

- Мультивалютность (одна валюта, без поля currency).
- Идempotency-ключи.
- Валидация/округление scale у decimal.
- Серверная блокировка или предупреждение при нарушении Limitation.

---

## 2. Терминология

| Термин | Определение |
|---|---|
| **Posting** | Движение средств по одному счёту в рамках транзакции: `(AccountId, Amount)`. Знак Amount кодирует направление. |
| **Storage** | Денежное хранилище (кошелёк, карта). `EndpointType = Storage`. |
| **Category** | Область трат (продукты, транспорт). `EndpointType = Category`. |
| **System** | Скрытый счёт для балансировки Adjustment. `EndpointType = System`. Создаётся при регистрации, не виден в API-списках. |
| **User posting** | Проводка, переданная клиентом. Не относится к System. |
| **System posting** | Проводка, автоматически добавленная сервером для System-счёта. |

---

## 3. Типы конечных точек (EndpointType)

### 3.1 Enum

```csharp
// DB: integer (0, 1, 2)
// API: string ("Storage", "Category", "System")
public enum EndpointType
{
    Storage  = 0,
    Category = 1,
    System   = 2,
}
```

### 3.2 Маппинг

| API (string) | DB (integer) | Описание |
|---|---|---|
| `"Storage"` | `0` | Денежное хранилище |
| `"Category"` | `1` | Область трат |
| `"System"` | `2` | Скрытый системный счёт |

### 3.3 Миграция с `IsStorage: bool`

Поле `IsStorage` на `TransactionEndpoint` заменяется на `EndpointType`.
Обратная совместимость не требуется — миграция односторонняя:

| Было (`IsStorage`) | Стало (`EndpointType`) |
|---|---|
| `true` | `Storage (0)` |
| `false` | `Category (1)` |

Новые записи `System (2)` создаются только сервером при регистрации.

---

## 4. Типы транзакций (TransactionType)

### 4.1 Enum

```csharp
// DB: integer
// API: string
public enum TransactionType
{
    Transfer             = 0,
    Expense              = 1,
    AdjustmentIncrease   = 2,
    AdjustmentDecrease   = 3,
}
```

### 4.2 Маппинг

| API (string) | DB (integer) | Описание |
|---|---|---|
| `"Transfer"` | `0` | Перевод между хранилищами |
| `"Expense"` | `1` | Трата из хранилища в категории |
| `"AdjustmentIncrease"` | `2` | Корректировка баланса вверх |
| `"AdjustmentDecrease"` | `3` | Корректировка баланса вниз |

Примечание: существующие значения `Income` и `ForTest` удаляются из enum.
Миграция данных — см. раздел 12.

---

## 5. API: Create Transaction

### 5.1 Request

```
POST /api/v1/transactions
```

```jsonc
{
  "postings": [
    { "accountId": "guid", "amount": -100.50 },
    { "accountId": "guid", "amount":  100.50 }
  ]
}
```

| Поле | Тип | Обязательное | Описание |
|---|---|---|---|
| `postings` | `PostingDto[]` | Да | Список пользовательских проводок |
| `postings[].accountId` | `Guid` | Да | Идентификатор счёта |
| `postings[].amount` | `decimal` | Да | Сумма со знаком (минус — списание, плюс — зачисление) |

### 5.2 Response (201 Created)

```jsonc
{
  "id": "guid",
  "type": "Transfer",            // string code
  "madeAt": "2026-09-21T12:00:00Z",
  "postings": [
    { "id": "guid", "accountId": "guid", "amount": -100.50 },
    { "id": "guid", "accountId": "guid", "amount":  100.50 }
  ]
}
```

**System-постинги исключены из публичного ответа.** Клиент видит только
user postings.

### 5.3 Response (Error)

```jsonc
{
  "code": "DUPLICATE_ACCOUNT_ID",
  "message": "Account <guid> referenced more than once in postings"
}
```

---

## 6. Правила классификации

Сервер классифицирует транзакцию **после** валидации проводок, анализируя
типы счетов (Storage / Category) и знаки сумм.

### 6.1 Transfer

| Условие | Значение |
|---|---|
| Все user postings | Только на Storage-счетах |
| Знаки | Минимум один отрицательный и минимум один положительный |
| Сумма user postings | `= 0` |
| System posting | Не добавляется |
| Limitation | **Не применяется** |

Пример:
```
Storage A: -500
Storage B: +500
→ Transfer
```

### 6.2 Expense

| Условие | Значение |
|---|---|
| User postings | Отрицательные на Storage + положительные на Category |
| Сумма user postings | `= 0` |
| System posting | Не добавляется |
| Limitation | Не применяется (сервер не блокирует и не предупреждает) |

Пример:
```
Storage A: -300
Category X: +200
Category Y: +100
→ Expense
```

### 6.3 AdjustmentIncrease

| Условие | Значение |
|---|---|
| Все user postings | Только на Storage-счетах |
| Знаки | Все положительные |
| Сумма user postings | `> 0` |
| System posting | Сервер добавляет одну: `System: -sum` |

Пример:
```
Storage A: +1000
System:    -1000  (добавлен сервером)
→ AdjustmentIncrease
```

### 6.4 AdjustmentDecrease

| Условие | Значение |
|---|---|
| Все user postings | Только на Storage-счетах |
| Знаки | Все отрицательные |
| Сумма user postings | `< 0` |
| System posting | Сервер добавляет одну: `System: -sum` (положительное значение) |

Пример:
```
Storage A: -500
System:    +500  (добавлен сервером)
→ AdjustmentDecrease
```

### 6.5 Алгоритм классификации (псевдокод)

```
1. Загрузить EndpointType для каждого accountId из postings.
2. Определить множества:
     storagePostings  = postings where endpointType == Storage
     categoryPostings = postings where endpointType == Category
3. Вычислить userSum = sum(postings.amount)

4. Если categoryPostings не пусто:
     Если storagePostings не пусто
        И storagePostings все отрицательные
        И categoryPostings все положительные
        И userSum == 0
     → Expense
     Иначе → ошибка INVALID_POSTING_COMBINATION

5. Если categoryPostings пусто (только Storage):
     storageSigns = distinct signs of storagePostings.amount

     Если storageSigns == {Negative, Positive} И userSum == 0
     → Transfer

     Если storageSigns == {Positive} И userSum > 0
     → AdjustmentIncrease (добавить System posting: -userSum)

     Если storageSigns == {Negative} И userSum < 0
     → AdjustmentDecrease (добавить System posting: -userSum)

     Иначе → ошибка INVALID_POSTING_COMBINATION
```

---

## 7. System-счёт

### 7.1 Создание

При регистрации пользователя (`UserService.RegistrationAsync`) создаётся
один TransactionEndpoint с:

| Поле | Значение |
|---|---|
| `UserId` | `user.Id` |
| `Name` | `"System"` |
| `EndpointType` | `System (2)` |
| `Value` | `0.0m` |
| `Limitation` | `null` |

### 7.2 Видимость

| Контекст | System виден? |
|---|---|
| `GET /api/v1/accounts` (список счетов) | Нет |
| `GET /api/v1/accounts/{id}` (детали) | Нет (404 если id = System) |
| `POST /api/v1/transactions` response | Нет (System postings исключены) |
| `GET /api/v1/transactions/{id}` response | Нет (System postings исключены) |
| Внутренняя логика (баланс, проводки) | Да |

### 7.3 Ограничения

- Клиент **не может** указать System accountId в postings (ошибка
  `SYSTEM_ACCOUNT_NOT_ALLOWED`).
- Сервер использует System accountId только для Adjustment-балансировки.

---

## 8. Валидация

### 8.1 Структурная (до классификации)

| Правило | Код ошибки | HTTP |
|---|---|---|
| `postings` не пустой | `POSTINGS_EMPTY` | 400 |
| `postings.Count <= MaxPostingsPerTransaction` | `POSTINGS_LIMIT_EXCEEDED` | 400 |
| `accountId` не Guid.Empty | `INVALID_ACCOUNT_ID` | 400 |
| `amount != 0` для каждой проводки | `ZERO_AMOUNT` | 400 |
| Нет дубликатов `accountId` внутри postings | `DUPLICATE_ACCOUNT_ID` | 400 |
| Все accountId существуют, принадлежат пользователю, не soft-deleted | `ACCOUNT_NOT_FOUND` | 404 |
| accountId не ссылается на System-счёт | `SYSTEM_ACCOUNT_NOT_ALLOWED` | 400 |

### 8.2 Семантическая (классификация)

| Правило | Код ошибки | HTTP |
|---|---|---|
| Комбинация типов и знаков не соответствует ни одному типу | `INVALID_POSTING_COMBINATION` | 400 |

### 8.3 MaxPostingsPerTransaction

- Источник: `ApiSettings:PostingSettings:MaxPostingsPerTransaction`
- Значение по умолчанию: `100`
- Чтение: **startup-only** (при старте приложения через `IOptions<ApiSettings>`)
- Валидация: если `postings.Count > MaxPostingsPerTransaction` → `POSTINGS_LIMIT_EXCEEDED`

```csharp
public sealed class ApiSettings
{
    public PostingSettings PostingSettings { get; set; } = new();
}

public sealed class PostingSettings
{
    public int MaxPostingsPerTransaction { get; set; } = 100;
}
```

```jsonc
// appsettings.json
{
  "ApiSettings": {
    "PostingSettings": {
      "MaxPostingsPerTransaction": 100
    }
  }
}
```

### 8.4 Что НЕ валидируется

| Проверка | Обоснование |
|---|---|
| Scale / precision decimal | Нет валидации и округления |
| Отрицательный баланс после транзакции | Разрешён |
| Limitation | Только UI-концепция, сервер не блокирует и не предупреждает |
| Transfer не применяет Limitation | Явно исключено |
| Idempotency | Не реализована |

---

## 9. Валюты

- Одна валюта на всю систему (рубли / единицы).
- Поле `currency` отсутствует на всех уровнях (API, DB, domain).
- Все `amount` — `decimal` без привязки к валюте.

---

## 10. Атомарность и сохранение

- Используется существующий `IUnitOfWork.SaveChangesAsync(token)`.
- Транзакция БД открывается EF Core автоматически при `SaveChanges`.
- Все Posting + Transaction + обновление Value на счетах — в одном
  `SaveChangesAsync` вызове.
- Rollback при любой ошибке — штатный механизм EF Core.
- Явное управление `IDbContextTransaction` не требуется.

---

## 11. Обработка ошибок

### 11.1 Формат

Все ошибки возвращаются в едином формате:

```jsonc
{
  "code": "ERROR_CODE_STRING",
  "message": "Human-readable description"
}
```

### 11.2 Таблица стабильных кодов

| Код | HTTP | Описание |
|---|---|---|
| `POSTINGS_EMPTY` | 400 | Список проводок пуст |
| `POSTINGS_LIMIT_EXCEEDED` | 400 | Превышен лимит количества проводок |
| `INVALID_ACCOUNT_ID` | 400 | accountId = Guid.Empty |
| `ZERO_AMOUNT` | 400 | Проводка с amount = 0 |
| `DUPLICATE_ACCOUNT_ID` | 400 | Дублирующийся accountId в postings |
| `ACCOUNT_NOT_FOUND` | 404 | Счёт не найден, не принадлежит пользователю или soft-deleted |
| `SYSTEM_ACCOUNT_NOT_ALLOWED` | 400 | Клиент указал System-счёт в postings |
| `INVALID_POSTING_COMBINATION` | 400 | Комбинация не соответствует ни одному типу |
| `TRANSACTION_NOT_FOUND` | 404 | Транзакция не найдена |
| `UNAUTHORIZED` | 401 | Неавторизованный запрос |

Коды стабильны: клиент может парсить `code` для программной обработки.
`message` может меняться без breaking change.

---

## 12. Миграция данных

### 12.1 TransactionEndpoint

```sql
-- Добавить колонку
ALTER TABLE "TransactionEndpoints"
  ADD COLUMN "EndpointType" integer NOT NULL DEFAULT 0;

-- Миграция данных
UPDATE "TransactionEndpoints"
  SET "EndpointType" = CASE
    WHEN "IsStorage" = true THEN 0   -- Storage
    ELSE 1                            -- Category
  END;

-- Создать System-счета для существующих пользователей
INSERT INTO "TransactionEndpoints" ("Id", "UserId", "Name", "EndpointType", "Value", "Limitation", "CreatedAt")
SELECT
  gen_random_uuid(),
  u."Id",
  'System',
  2,  -- System
  0.0,
  NULL,
  NOW()
FROM "Users" u;

-- Удалить старую колонку
ALTER TABLE "TransactionEndpoints" DROP COLUMN "IsStorage";
```

### 12.2 TransactionType

Существующие значения `Income (4)` и `ForTest (5)` удаляются.
Если в БД есть записи с этими типами — требуется предварительный анализ
и ручная миграция (за рамками данного документа).

---

## 13. Acceptance Criteria

### AC-1: Transfer

- [ ] Даны два Storage-счёта. Клиент отправляет postings: `[{A, -500}, {B, +500}]`.
- [ ] Сервер создаёт Transaction с `Type = Transfer`.
- [ ] Создаются 2 Posting. System posting не создаётся.
- [ ] Response содержит 2 postings (без System).
- [ ] Баланс A уменьшен на 500, баланс B увеличен на 500.
- [ ] Limitation не проверяется.

### AC-2: Expense

- [ ] Даны Storage + два Category. Клиент отправляет: `[{S, -300}, {C1, +200}, {C2, +100}]`.
- [ ] Сервер создаёт Transaction с `Type = Expense`.
- [ ] Создаются 3 Posting. System posting не создаётся.
- [ ] Response содержит 3 postings.
- [ ] Баланс S уменьшен на 300, балансы C1/C2 увеличены.

### AC-3: AdjustmentIncrease

- [ ] Дан один Storage. Клиент отправляет: `[{S, +1000}]`.
- [ ] Сервер создаёт Transaction с `Type = AdjustmentIncrease`.
- [ ] Создаются 2 Posting: user `{S, +1000}` + system `{System, -1000}`.
- [ ] Response содержит 1 posting (только user, без System).
- [ ] Баланс S увеличен на 1000.

### AC-4: AdjustmentDecrease

- [ ] Дан один Storage. Клиент отправляет: `[{S, -500}]`.
- [ ] Сервер создаёт Transaction с `Type = AdjustmentDecrease`.
- [ ] Создаются 2 Posting: user `{S, -500}` + system `{System, +500}`.
- [ ] Response содержит 1 posting (только user, без System).
- [ ] Баланс S уменьшен на 500.

### AC-5: Валидация — дубликат accountId

- [ ] Клиент отправляет `[{A, -100}, {A, +100}]`.
- [ ] Ответ: 400, `code = "DUPLICATE_ACCOUNT_ID"`.

### AC-6: Валидация — превышение лимита

- [ ] `MaxPostingsPerTransaction = 100`. Клиент отправляет 101 posting.
- [ ] Ответ: 400, `code = "POSTINGS_LIMIT_EXCEEDED"`.

### AC-7: Валидация — soft-deleted счёт

- [ ] Счёт soft-deleted (DeletedAt != null).
- [ ] Ответ: 404, `code = "ACCOUNT_NOT_FOUND"`.

### AC-8: Валидация — чужой счёт

- [ ] AccountId принадлежит другому пользователю.
- [ ] Ответ: 404, `code = "ACCOUNT_NOT_FOUND"`.

### AC-9: Валидация — System accountId от клиента

- [ ] Клиент указывает System-счёт в postings.
- [ ] Ответ: 400, `code = "SYSTEM_ACCOUNT_NOT_ALLOWED"`.

### AC-10: Отрицательный баланс разрешён

- [ ] Storage с балансом 100. Клиент отправляет `[{S, -500}]` (AdjustmentDecrease).
- [ ] Транзакция создаётся успешно. Баланс S = -400.

### AC-11: Публичный response исключает System postings

- [ ] AdjustmentIncrease с 1 user posting.
- [ ] Response содержит ровно 1 posting (без System).

### AC-12: Атомарность

- [ ] При ошибке валидации после частичной обработки — ни одна сущность не сохранена.
- [ ] `SaveChangesAsync` вызывается один раз, после полной валидации.

### AC-13: System-счёт не виден в списках

- [ ] `GET /api/v1/accounts` не возвращает System-счёт.
- [ ] `GET /api/v1/accounts/{systemId}` возвращает 404.

### AC-14: decimal без scale валидации

- [ ] Клиент отправляет `amount = 0.123456789`.
- [ ] Значение сохраняется как есть, без округления.

---

## 14. Test Matrix

### 14.1 Классификация

| # | Postings | Ожидаемый тип | System posting |
|---|---|---|---|
| T01 | `[Storage A: -100, Storage B: +100]` | Transfer | Нет |
| T02 | `[Storage A: -50, Storage B: -50, Storage C: +100]` | Transfer | Нет |
| T03 | `[Storage A: -300, Category X: +300]` | Expense | Нет |
| T04 | `[Storage A: -300, Category X: +200, Category Y: +100]` | Expense | Нет |
| T05 | `[Storage A: +1000]` | AdjustmentIncrease | `System: -1000` |
| T06 | `[Storage A: +500, Storage B: +500]` | AdjustmentIncrease | `System: -1000` |
| T07 | `[Storage A: -500]` | AdjustmentDecrease | `System: +500` |
| T08 | `[Storage A: -200, Storage B: -300]` | AdjustmentDecrease | `System: +500` |

### 14.2 Ошибки классификации

| # | Postings | Ожидаемый код ошибки |
|---|---|---|
| T09 | `[Storage A: +100, Storage B: -50]` (sum != 0, mixed signs) | `INVALID_POSTING_COMBINATION` |
| T10 | `[Storage A: -100, Category X: -100]` (Category отрицательный) | `INVALID_POSTING_COMBINATION` |
| T11 | `[Category X: +100]` (только Category, без Storage) | `INVALID_POSTING_COMBINATION` |
| T12 | `[Storage A: +100, Category X: +50]` (mixed Storage/Category, sum != 0) | `INVALID_POSTING_COMBINATION` |
| T13 | `[Storage A: -100, Storage B: +50, Category X: +50]` (Storage mixed signs + Category) | `INVALID_POSTING_COMBINATION` |
| T14 | `[]` (пустой список) | `POSTINGS_EMPTY` |

### 14.3 Валидация

| # | Условие | Ожидаемый код |
|---|---|---|
| T15 | `postings.Count = 101`, лимит = 100 | `POSTINGS_LIMIT_EXCEEDED` |
| T16 | `accountId = Guid.Empty` | `INVALID_ACCOUNT_ID` |
| T17 | `amount = 0` | `ZERO_AMOUNT` |
| T18 | Дубликат accountId | `DUPLICATE_ACCOUNT_ID` |
| T19 | AccountId не существует | `ACCOUNT_NOT_FOUND` |
| T20 | AccountId принадлежит другому пользователю | `ACCOUNT_NOT_FOUND` |
| T21 | AccountId — soft-deleted счёт | `ACCOUNT_NOT_FOUND` |
| T22 | AccountId — System-счёт | `SYSTEM_ACCOUNT_NOT_ALLOWED` |

### 14.4 Граничные значения

| # | Условие | Ожидаемое поведение |
|---|---|---|
| T23 | `amount = 0.001` (малый scale) | Сохраняется как есть |
| T24 | `amount = 999999999999.999999999999` (большой scale) | Сохраняется как есть |
| T25 | `amount = -0.01` (минимальный отрицательный) | Валидно |
| T26 | Баланс уходит в минус | Разрешено, без ошибок |
| T27 | `postings.Count = 100` (ровно лимит) | Валидно |
| T28 | `postings.Count = 1` (один posting, Adjustment) | Валидно, System posting добавляется |

### 14.5 Transfer и Limitation

| # | Условие | Ожидаемое поведение |
|---|---|---|
| T29 | Transfer, Storage имеет `Limitation = 100`, баланс станет ниже лимита | Транзакция проходит, Limitation не применяется |
| T30 | Expense, Storage имеет `Limitation = 100`, баланс станет ниже лимита | Транзакция проходит, Limitation не применяется (сервер не блокирует) |

### 14.6 Видимость System

| # | Условие | Ожидаемое поведение |
|---|---|---|
| T31 | `GET /api/v1/accounts` | System не в списке |
| T32 | `GET /api/v1/accounts/{systemId}` | 404 |
| T33 | `GET /api/v1/transactions/{id}` для Adjustment | System postings не в ответе |
| T34 | `POST /api/v1/transactions` response для Adjustment | System postings не в ответе |

### 14.7 Атомарность

| # | Условие | Ожидаемое поведение |
|---|---|---|
| T35 | Ошибка валидации после загрузки счетов | Ни одна сущность не сохранена |
| T36 | `SaveChangesAsync` откатывает при DB error | Все изменения откачены |

### 14.8 System-счёт при регистрации

| # | Условие | Ожидаемое поведение |
|---|---|---|
| T37 | Новый пользователь зарегистрирован | System-счёт создан с `EndpointType = System`, `Value = 0` |
| T38 | System-счёт создан | `Name = "System"`, `Limitation = null` |

---

## 15. Открытые вопросы

Отсутствуют. Дизайн полностью согласован.

---

## 16. Changelog

| Дата | Изменение |
|---|---|
| 2026-09-21 | Первоначальная версия, согласована с командой |
