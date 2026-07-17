# План: обновление схемы БД (bd.md)

Ветка: `feature/db-schema-update`

## Решения (зафиксированы с пользователем)
- `TransactionEndpoint.Value` **НЕ удаляется** (остаётся в схеме).
- `Posting.Amount` несёт **знак**: минус = расход со счёта, плюс = приход на счёт.
- Транзакция содержит **произвольное число постингов (1..N)**, сумма постингов транзакции **= 0** (double-entry, сбалансировано).
- `DailyExpenseCategorie.totalAmount` заполняется **фоновой задачей** (аналог ClearCategoryCacheService).
- `DailyExpenseCategorie.categorieId` — FK на `TransactionEndpoint` (область трат, isStorage=false); `day` — DateTime с точностью до дня.
- Миграция данных НЕ нужна (проект только в разработке).

## Изменения схемы
1. `Transaction`: убрать `SourceAccountId`, `DestinationAccountId`, `Amount`; добавить `ICollection<Posting> Postings`.
2. Новая `Posting`: `Id (Guid PK)`, `AccountId (Guid FK→TransactionEndpoint)`, `TransactionId (Guid FK→Transaction)`, `Amount (double, signed)`, `DeletedAt?`; навигации `Account`, `Transaction`.
3. Новая `DailyExpenseCategorie`: составной PK `(categorieId, day)`, `TotalAmount (double)`, навигация `Category (TransactionEndpoint)`.
4. `TransactionEndpoint`: заменить `OutgoingTransactions`/`IncomingTransactions` на `ICollection<Posting> Postings` (и, возможно, `ICollection<DailyExpenseCategorie> DailyExpenseCategories`).

## Этап 1 (ВЫПОЛНЕН) — схема + миграция, БЕЗ сервисов/тестов
- [x] Сущность `Posting` (DAL/Entities/)
- [x] Сущность `DailyExpenseCategorie` (DAL/Entities/)
- [x] Изменить `Transaction` (убрать Source/Destination/Amount, добавить Postings)
- [x] Изменить `TransactionEndpoint` (навигации → Postings + DailyExpenseCategories)
- [x] `PostingConfiguration` (DAL/Entity.Configuration/)
- [x] `DailyExpenseCategorieConfiguration` (составной ключ)
- [x] `TransactionConfiguration` (убрать связи Source/Destination; 1-to-many → Postings)
- [x] `TransactionEndpointConfiguration` (1-to-many → Postings)
- [x] `SmartWalletContext` (DbSet<Posting>, DbSet<DailyExpenseCategorie>)
- [x] `SmartWalletContextFactory` (IDesignTimeDbContextFactory — для миграций без startup-project)
- [x] Добавлен Microsoft.EntityFrameworkCore.Design в DAL/Context.csproj
- [x] Миграция: `20260716103856_RefactorToPostings` (drop Amount/Source/Destination из Transaction; create Posting, DailyExpenseCategorie)
- [x] Сборка DAL/Context успешна (0 ошибок)

## Этап 2 (СЛЕДУЮЩИЙ, после ревью) — логика
- [ ] ITransactionRepository / TransactionRepository: GetBalance по Posting, DeleteByEndpoint, аналитика через Posting
- [ ] TransactionService.CreateAsync / DeleteAsync: генерация Posting, валидация суммы=0, лимиты по постингам
- [ ] Модели Create/Update/Transaction + API модели + валидаторы (постинги вместо Source/Destination/Amount)
- [ ] ServiceModelMapper: маппинг Posting
- [ ] Фоновая задача заполнения DailyExpenseCategorie (расширить/заменить ClearCategoryCacheService)
- [ ] Тесты: TransactionServiceTests, FinancialAnalyticsServiceTests, репозиторные

## Этап 3 (в конце) — документация
- [ ] README.md: диаграмма БД из bd.md
