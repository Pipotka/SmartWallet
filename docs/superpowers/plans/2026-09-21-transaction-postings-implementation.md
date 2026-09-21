REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development

# План реализации postings-based транзакций

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Перевести API создания транзакции на модель postings: клиент передаёт список проводок `(accountId, amount)`, сервер валидирует их, классифицирует тип транзакции (Transfer / Expense / AdjustmentIncrease / AdjustmentDecrease) автоматически, при необходимости добавляет скрытую проводку на System-счёт и возвращает публичный ответ без System-постингов.

**Architecture:** Домен `TransactionEndpoint` получает поле `EndpointType` (Storage / Category / System) вместо `IsStorage`. EF Core хранит `EndpointType` как integer через стандартный value converter. System-счёт создаётся только при регистрации пользователя и скрыт из публичных API. Классификация транзакции выполняется приватными методами внутри `TransactionService`. Все ошибки возвращаются в едином DTO `ApiErrorApiModel` (code + message); непредвиденные исключения → 500 `internal_error`. Глобальный JSON-конвертер enum (`JsonStringEnumConverter`) зарегистрирован в `Program.cs`.

**Tech Stack:** C# .NET 8, ASP.NET Core controllers, EF Core 8 + Npgsql + PostgreSQL, AutoMapper, FluentValidation, Repository / UnitOfWork, xUnit / Moq / FluentAssertions / InMemory tests.

---

## Подготовка

### Task 0.1: Создать ветку

- [ ] **Step 1: Создать feature-ветку**

**Команда:**
```bash
git checkout -b feature/transaction-postings
```

**Ожидаемый результат:**
```
Switched to a new branch 'feature/transaction-postings'
```

### Task 0.2: Проверить базовое состояние тестов

- [ ] **Step 1: Запустить все тесты**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.sln --no-restore
```

**Ожидаемый результат:**
```
Test Run Successful.
Total tests: ...
```

---

## Фаза 1. EndpointType enum и замена IsStorage

### Task 1.1: Создать enum EndpointType

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entities/Enums/EndpointType.cs`

- [ ] **Step 1: Написать падающий тест**

**File:** `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/EndpointTypeTests.cs`

```csharp
using FluentAssertions;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

public class EndpointTypeTests
{
    [Fact]
    public void EndpointType_ShouldHaveExpectedIntegerValues()
    {
        ((int)EndpointType.Storage).Should().Be(0);
        ((int)EndpointType.Category).Should().Be(1);
        ((int)EndpointType.System).Should().Be(2);
    }
}
```

- [ ] **Step 2: Запустить тест, убедиться что падает**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter EndpointTypeTests
```

**Ожидаемый результат:**
```
Error: The type or namespace name 'EndpointType' could not be found
```

- [ ] **Step 3: Создать enum**

```csharp
namespace Nasurino.SmartWallet.Entities;

public enum EndpointType
{
    Storage = 0,
    Category = 1,
    System = 2
}
```

- [ ] **Step 4: Запустить тест, убедиться что проходит**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter EndpointTypeTests
```

**Ожидаемый результат:**
```
Test Run Successful.
```

- [ ] **Step 5: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entities/Enums/EndpointType.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/EndpointTypeTests.cs
git commit -m "feat: add EndpointType enum with stable integer values"
```

### Task 1.2: Заменить IsStorage на EndpointType в сущности

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entities/TransactionEndpoint.cs`

- [ ] **Step 1: Прочитать текущий TransactionEndpoint.cs**

- [ ] **Step 2: Заменить свойство IsStorage на EndpointType**

```csharp
/// <summary>
/// Тип конечной точки (Storage / Category / System)
/// </summary>
public EndpointType EndpointType { get; set; }
```

- [ ] **Step 3: Собрать проект Entities**

**Команда:**
```bash
dotnet build /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entities/Entities.csproj
```

**Ожидаемый результат:**
```
Build succeeded.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entities/TransactionEndpoint.cs
git commit -m "feat: replace IsStorage with EndpointType in TransactionEndpoint entity"
```

### Task 1.3: Настроить EF Core mapping для EndpointType

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entity.Configuration/TransactionEndpointConfiguration.cs`

- [ ] **Step 1: Прочитать текущий TransactionEndpointConfiguration.cs**

- [ ] **Step 2: Добавить явное преобразование enum в integer**

```csharp
builder.Property(x => x.EndpointType)
    .HasConversion<int>();
```

- [ ] **Step 3: Собрать проект Entity.Configuration**

**Команда:**
```bash
dotnet build /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entity.Configuration/Entity.Configuration.csproj
```

**Ожидаемый результат:**
```
Build succeeded.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Entity.Configuration/TransactionEndpointConfiguration.cs
git commit -m "feat: configure EndpointType integer storage via EF value converter"
```

### Task 1.4: Обновить TransactionEndpointModel

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/Models/TransactionEndpointModel.cs`

- [ ] **Step 1: Заменить bool IsStorage на EndpointType EndpointType**

```csharp
/// <summary>
/// Тип конечной точки
/// </summary>
public EndpointType EndpointType { get; set; }
```

- [ ] **Step 2: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/Models/TransactionEndpointModel.cs
git commit -m "feat: use EndpointType in TransactionEndpointModel"
```

### Task 1.5: Обновить CreateTransactionEndpointModel

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/CreateModels/CreateTransactionEndpointModel.cs`

- [ ] **Step 1: Заменить bool IsStorage на EndpointType EndpointType**

```csharp
/// <summary>
/// Тип конечной точки
/// </summary>
public EndpointType EndpointType { get; set; }
```

- [ ] **Step 2: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/CreateModels/CreateTransactionEndpointModel.cs
git commit -m "feat: use EndpointType in CreateTransactionEndpointModel"
```

### Task 1.6: Обновить API-модели конечных точек

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/CashVault/TransactionEndpointApiModel.cs`
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/CashVault/CreateTransactionEndpointApiModel.cs`

- [ ] **Step 1: Заменить bool IsStorage на EndpointType EndpointType в обоих файлах**

```csharp
/// <summary>
/// Тип конечной точки
/// </summary>
public EndpointType EndpointType { get; set; }
```

- [ ] **Step 2: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/CashVault/TransactionEndpointApiModel.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/CashVault/CreateTransactionEndpointApiModel.cs
git commit -m "feat: expose EndpointType in endpoint API models"
```

### Task 1.7: Обновить мапперы

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/AutoMappers/ServiceModelMapper.cs`
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/AutoMappers/ApiModelMapper.cs`

- [ ] **Step 1: Прочитать оба файла**

- [ ] **Step 2: Удалить любые явные маппинги/игноры для IsStorage; оставить EndpointType на автоматический маппинг**

- [ ] **Step 3: Собрать решение**

**Команда:**
```bash
dotnet build /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.sln
```

**Ожидаемый результат:**
```
Build succeeded.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/AutoMappers/ServiceModelMapper.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/AutoMappers/ApiModelMapper.cs
git commit -m "feat: map EndpointType in AutoMapper profiles"
```

### Task 1.8: Обновить TransactionEndpointRepository

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context.Repository/TransactionEndpointRepository.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Заменить использование IsStorage на EndpointType**

```csharp
Task<List<TransactionEndpoint>> ITransactionEndpointRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    => Storage.Read<TransactionEndpoint>()
        .NotDeleted()
        .Where(x => x.UserId == userId)
        .OrderByDescending(x => x.EndpointType)
        .ToListAsync(cancellationToken);

Task ITransactionEndpointRepository.ClearCategoryValueCacheAsync(CancellationToken cancellationToken)
    => Storage.Read<TransactionEndpoint>()
        .Where(x => x.EndpointType == EndpointType.Category && x.Value > 0)
        .ExecuteUpdateAsync(setter => setter.SetProperty(x => x.Value, 0));
```

- [ ] **Step 3: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context.Repository/TransactionEndpointRepository.cs
git commit -m "feat: use EndpointType in repository queries"
```

### Task 1.9: Обновить TransactionEndpointService

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/TransactionEndpointService.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Заменить проверку !transactionEndpoint.IsStorage**

```csharp
if (transactionEndpoint.EndpointType == EndpointType.Category)
{
    _transactionRepository.DeleteTransactionsByTransactionEndpointIdAndDateRange(transactionEndpoint.Id);
}
```

- [ ] **Step 3: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/TransactionEndpointService.cs
git commit -m "feat: use EndpointType in endpoint service"
```

### Task 1.10: Обновить тесты TransactionEndpointRepositoryTests

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Context.Repository.Tests/TransactionEndpointRepositoryTests.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Заменить IsStorage = true/false на EndpointType = EndpointType.Storage/Category и проверку сортировки**

- [ ] **Step 3: Запустить тесты**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Context.Repository.Tests/Context.Repository.Tests.csproj --filter TransactionEndpointRepositoryTests
```

**Ожидаемый результат:**
```
Test Run Successful.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Context.Repository.Tests/TransactionEndpointRepositoryTests.cs
git commit -m "test: update endpoint repository tests for EndpointType"
```

---

## Фаза 2. Конфигурация и глобальный JSON-конвертер enum

### Task 2.1: Создать PostingSettings

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.Options/PostingSettings.cs`

- [ ] **Step 1: Создать файл**

```csharp
namespace Nasurino.SmartWallet.Options;

public sealed class PostingSettings
{
    public int MaxPostingsPerTransaction { get; set; } = 100;
}
```

### Task 2.2: Создать ApiSettings

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.Options/ApiSettings.cs`

- [ ] **Step 1: Создать файл**

```csharp
namespace Nasurino.SmartWallet.Options;

public sealed class ApiSettings
{
    public PostingSettings PostingSettings { get; set; } = new();
}
```

- [ ] **Step 2: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.Options/PostingSettings.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.Options/ApiSettings.cs
git commit -m "feat: add ApiSettings and PostingSettings options classes"
```

### Task 2.3: Зарегистрировать ApiSettings и валидацию

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Program.cs`

- [ ] **Step 1: Прочитать текущий Program.cs**

- [ ] **Step 2: Добавить регистрацию после существующих Configure<...>**

```csharp
builder.Services.AddOptions<ApiSettings>()
    .Bind(builder.Configuration.GetSection("ApiSettings"))
    .Validate(settings => settings.PostingSettings.MaxPostingsPerTransaction >= 2,
        "ApiSettings:PostingSettings:MaxPostingsPerTransaction must be >= 2");

builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ApiSettings>>().Value);
```

### Task 2.4: Добавить секцию в appsettings.json

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/appsettings.json`

- [ ] **Step 1: Прочитать текущий appsettings.json**

- [ ] **Step 2: Добавить внутрь ApiSettings**

```json
"PostingSettings": {
  "MaxPostingsPerTransaction": 100
}
```

### Task 2.5: Зарегистрировать глобальный JSON-конвертер enum

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Program.cs`

- [ ] **Step 1: Изменить регистрацию контроллеров**

```csharp
builder.Services.AddControllers(x =>
{
    x.Filters.Add(typeof(SmartWalletExceptionFilter));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

- [ ] **Step 2: Добавить using**

```csharp
using System.Text.Json.Serialization;
```

- [ ] **Step 3: Собрать проект SmartWallet**

**Команда:**
```bash
dotnet build /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/SmartWallet.csproj
```

**Ожидаемый результат:**
```
Build succeeded.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Program.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/appsettings.json
git commit -m "feat: register ApiSettings validation and global enum JSON converter"
```

---

## Фаза 3. ApiErrorApiModel и фильтр исключений

### Task 3.1: Создать ApiErrorApiModel

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/ApiErrorApiModel.cs`

- [ ] **Step 1: Создать файл**

```csharp
namespace Nasurino.SmartWallet.Models;

public sealed class ApiErrorApiModel
{
    public required string Code { get; set; }
    public required string Message { get; set; }
}
```

### Task 3.2: Создать CodedServiceException

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Exceptions/CodedServiceException.cs`

- [ ] **Step 1: Создать файл**

```csharp
namespace Nasurino.SmartWallet.Service.Exceptions;

public sealed class CodedServiceException : ServiceException
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public CodedServiceException(string errorCode, string message, int statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
```

### Task 3.3: Переписать SmartWalletExceptionFilter

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Infrastructure/SmartWalletExceptionFilter.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Заменить содержимое файла**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nasurino.SmartWallet.Models;
using Nasurino.SmartWallet.Service.Exceptions;

namespace Nasurino.SmartWallet.Infrastructure;

public sealed class SmartWalletExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case CodedServiceException coded:
                SetResult(context, coded.StatusCode, coded.ErrorCode, coded.Message);
                return;

            case EntityNotFoundByIdServiceException<TransactionEndpoint> ex:
                SetResult(context, StatusCodes.Status404NotFound, "ACCOUNT_NOT_FOUND", ex.Message);
                return;

            case EntityNotFoundByIdServiceException<Transaction> ex:
                SetResult(context, StatusCodes.Status404NotFound, "TRANSACTION_NOT_FOUND", ex.Message);
                return;

            case EntityNotFoundServiceException ex:
                SetResult(context, StatusCodes.Status404NotFound, "not_found", ex.Message);
                return;

            case SmartWalletValidationException ex:
                SetResult(context, StatusCodes.Status400BadRequest, "VALIDATION_ERROR", ex.Message);
                return;

            case AuthenticationServiceException ex:
            case AuthorizationServiceException ex:
                SetResult(context, StatusCodes.Status401Unauthorized, "unauthorized", ex.Message);
                return;

            case EntityAccessServiceException ex:
                SetResult(context, StatusCodes.Status403Forbidden, "access_denied", ex.Message);
                return;

            case AccountBalanceLimitViolationException ex:
                SetResult(context, StatusCodes.Status409Conflict, "limit_violation", ex.Message);
                return;
        }

        SetResult(context, StatusCodes.Status500InternalServerError, "internal_error", "Внутренняя ошибка сервера");
    }

    private static void SetResult(ExceptionContext context, int statusCode, string code, string message)
    {
        context.ExceptionHandled = true;
        context.HttpContext.Response.StatusCode = statusCode;
        context.Result = new ObjectResult(new ApiErrorApiModel
        {
            Code = code,
            Message = message
        })
        {
            StatusCode = statusCode
        };
    }
}
```

### Task 3.4: Обновить ProducesResponseType во всех контроллерах

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Controllers/TransactionController.cs`
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Controllers/TransactionEndpointController.cs`
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Controllers/FinancialAnalyticsController.cs`
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Controllers/UserController.cs`

- [ ] **Step 1: Заменить `typeof(ApiExceptionDetails)` на `typeof(ApiErrorApiModel)` во всех атрибутах**

- [ ] **Step 2: Собрать проект SmartWallet**

**Команда:**
```bash
dotnet build /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/SmartWallet.csproj
```

**Ожидаемый результат:**
```
Build succeeded.
```

- [ ] **Step 3: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/ApiErrorApiModel.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Exceptions/CodedServiceException.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Infrastructure/SmartWalletExceptionFilter.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Controllers/*.cs
git commit -m "feat: introduce ApiErrorApiModel and coded exception filter"
```

### Task 3.5: Написать тест на фильтр (TDD)

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/SmartWalletExceptionFilterTests.cs`

- [ ] **Step 1: Написать падающий тест**

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Service.Exceptions;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

public class SmartWalletExceptionFilterTests
{
    [Fact]
    public void OnException_ShouldReturn500InternalError_ForUnhandledException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new InvalidOperationException("boom")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { Code = "internal_error", Message = "Внутренняя ошибка сервера" });
    }

    [Fact]
    public void OnException_ShouldReturnCodedError_ForCodedServiceException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new CodedServiceException("POSTINGS_EMPTY", "Список проводок пуст", StatusCodes.Status400BadRequest)
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
```

- [ ] **Step 2: Запустить тест**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter SmartWalletExceptionFilterTests
```

**Ожидаемый результат:**
```
Test Run Successful.
```

- [ ] **Step 3: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/SmartWalletExceptionFilterTests.cs
git commit -m "test: add exception filter tests for coded and unhandled errors"
```

---

## Фаза 4. Модели и мапперы для postings

### Task 4.1: Создать CreateTransactionPostingModel

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/CreateModels/CreateTransactionPostingModel.cs`

- [ ] **Step 1: Создать файл**

```csharp
namespace Nasurino.SmartWallet.Service.Models.CreateModels;

public sealed class CreateTransactionPostingModel
{
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }
}
```

### Task 4.2: Обновить CreateTransactionModel

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/CreateModels/CreateTransactionModel.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Удалить SourceAccountId, DestinationAccountId, Amount; добавить Postings**

```csharp
namespace Nasurino.SmartWallet.Service.Models.CreateModels;

public class CreateTransactionModel
{
    public Guid UserId { get; set; }

    public List<CreateTransactionPostingModel> Postings { get; set; } = [];
}
```

### Task 4.3: Создать CreateTransactionPostingApiModel

**Files:**
- Create: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/Transaction/CreateTransactionPostingApiModel.cs`

- [ ] **Step 1: Создать файл**

```csharp
namespace Nasurino.SmartWallet.Models.Transaction;

public sealed class CreateTransactionPostingApiModel
{
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }
}
```

### Task 4.4: Обновить CreateTransactionApiModel

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/Transaction/CreateTransactionApiModel.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Заменить содержимое**

```csharp
namespace Nasurino.SmartWallet.Models.Transaction;

public sealed class CreateTransactionApiModel
{
    public List<CreateTransactionPostingApiModel> Postings { get; set; } = [];
}
```

### Task 4.5: Обновить ApiModelMapper

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/AutoMappers/ApiModelMapper.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Добавить маппинги**

```csharp
CreateMap<CreateTransactionPostingApiModel, CreateTransactionPostingModel>(MemberList.Destination);
CreateMap<CreateTransactionApiModel, CreateTransactionModel>(MemberList.Destination)
    .ForMember(x => x.UserId, opt => opt.Ignore());
```

### Task 4.6: Сделать валидатор CreateTransactionModel no-op и удалить старый валидатор

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/Validators/SmartWalletValidateService.cs`
- Delete: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/Validators/CreateModelValidators/CreateTransactionModelValidator.cs`

- [ ] **Step 1: Прочитать SmartWalletValidateService.cs**

- [ ] **Step 2: Изменить ValidateAsync**

```csharp
async Task ISmartWalletValidateService.ValidateAsync<TModel>(TModel model, CancellationToken token)
{
    if (!_validators.TryGetValue(typeof(TModel), out var validator) || validator is null)
    {
        return;
    }

    var validationResult = await validator.ValidateAsync(new ValidationContext<TModel>(model), token);
    if (!validationResult.IsValid)
    {
        throw new SmartWalletValidationException(validationResult.Errors
            .Select(x => new PropertyValidationError(x.PropertyName, x.ErrorMessage)).ToList());
    }
}
```

- [ ] **Step 3: Удалить строку регистрации CreateTransactionModelValidator**

```csharp
_validators.Add(typeof(CreateTransactionModel), new CreateTransactionModelValidator());
```

- [ ] **Step 4: Удалить файл CreateTransactionModelValidator.cs**

**Команда:**
```bash
git rm /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/Validators/CreateModelValidators/CreateTransactionModelValidator.cs
```

### Task 4.7: Фильтровать System-постинги в маппере TransactionModel

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/AutoMappers/ServiceModelMapper.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Изменить маппинг Transaction -> TransactionModel**

```csharp
CreateMap<Transaction, TransactionModel>(MemberList.Destination)
    .ForMember(dest => dest.Postings, opt => opt.MapFrom(src => src.Postings
        .Where(p => p.Account == null || p.Account.EndpointType != EndpointType.System)
        .ToList()));
```

- [ ] **Step 3: Собрать решение**

**Команда:**
```bash
dotnet build /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.sln
```

**Ожидаемый результат:**
```
Build succeeded.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/CreateModels/CreateTransactionPostingModel.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Models/CreateModels/CreateTransactionModel.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/Transaction/CreateTransactionPostingApiModel.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Models/Transaction/CreateTransactionApiModel.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/AutoMappers/ApiModelMapper.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/AutoMappers/ServiceModelMapper.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/Validators/SmartWalletValidateService.cs
git commit -m "feat: postings-based create transaction models and mappers"
```

---

## Фаза 5. Классификация в TransactionService (TDD)

### Task 5.1: Написать падающие тесты TransactionService

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/TransactionServiceTests.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Заменить содержимое**

```csharp
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Services.AutoMappers;
using Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;
using Services.Contracts;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISmartWalletValidateService> _validateServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ITransactionEndpointRepository> _transactionEndpointRepositoryMock;
    private readonly Mock<IPostingRepository> _postingRepositoryMock;
    private readonly Mock<IBackgroundTaskSystemProvider> _backgroundTaskSystemProviderMock;
    private readonly ITransactionService _transactionService;
    private readonly Guid _systemEndpointId = Guid.NewGuid();
    private readonly Dictionary<Guid, TransactionEndpoint> _endpoints = new();

    public TransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        var mapper = new MapperConfiguration(conf => conf.AddProfile<ServiceModelMapper>()).CreateMapper();

        _userRepositoryMock = new Mock<IUserRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _transactionEndpointRepositoryMock = new Mock<ITransactionEndpointRepository>();
        _postingRepositoryMock = new Mock<IPostingRepository>();
        _backgroundTaskSystemProviderMock = new Mock<IBackgroundTaskSystemProvider>();

        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TransactionRepository).Returns(_transactionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TransactionEndpointRepository).Returns(_transactionEndpointRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.PostingRepository).Returns(_postingRepositoryMock.Object);

        var apiSettings = Options.Create(new ApiSettings
        {
            PostingSettings = new PostingSettings { MaxPostingsPerTransaction = 100 }
        });

        _transactionService = new TransactionService(
            _unitOfWorkMock.Object,
            _validateServiceMock.Object,
            mapper,
            _backgroundTaskSystemProviderMock.Object,
            apiSettings);

        _transactionEndpointRepositoryMock
            .Setup(r => r.GetByNameAndUserIdAsync(It.IsAny<Guid>(), "System", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid userId, string _, CancellationToken _) => new TransactionEndpoint
            {
                Id = _systemEndpointId,
                UserId = userId,
                Name = "System",
                EndpointType = EndpointType.System,
                Value = 0m
            });

        _transactionEndpointRepositoryMock
            .Setup(r => r.GetListByIdsAndUserIdAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, IReadOnlyCollection<Guid> ids, CancellationToken __) =>
                ids.Where(id => _endpoints.ContainsKey(id)).Select(id => _endpoints[id]).ToList());

        _transactionRepositoryMock
            .Setup(r => r.GetStorageBalancesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> ids, CancellationToken _) => ids.ToDictionary(id => id, _ => 0m));

        _transactionRepositoryMock
            .Setup(r => r.GetCategoryBalancesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> ids, CancellationToken _) => ids.ToDictionary(id => id, _ => 0m));
    }

    [Fact]
    public async Task CreateShouldCreateTransfer()
    {
        var userId = Guid.NewGuid();
        var source = Guid.NewGuid();
        var dest = Guid.NewGuid();
        AddEndpoint(source, userId, EndpointType.Storage);
        AddEndpoint(dest, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = source, Amount = -500m },
                new CreateTransactionPostingModel { AccountId = dest, Amount = 500m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.Transfer);
        result.Postings.Should().HaveCount(2);
        result.Postings.Should().ContainSingle(p => p.AccountId == source && p.Amount == -500m);
        result.Postings.Should().ContainSingle(p => p.AccountId == dest && p.Amount == 500m);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateShouldCreateExpense()
    {
        var userId = Guid.NewGuid();
        var storage = Guid.NewGuid();
        var cat1 = Guid.NewGuid();
        var cat2 = Guid.NewGuid();
        AddEndpoint(storage, userId, EndpointType.Storage);
        AddEndpoint(cat1, userId, EndpointType.Category);
        AddEndpoint(cat2, userId, EndpointType.Category);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = storage, Amount = -300m },
                new CreateTransactionPostingModel { AccountId = cat1, Amount = 200m },
                new CreateTransactionPostingModel { AccountId = cat2, Amount = 100m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.Expense);
        result.Postings.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateShouldCreateAdjustmentIncreaseAndHideSystemPosting()
    {
        var userId = Guid.NewGuid();
        var storage = Guid.NewGuid();
        AddEndpoint(storage, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = storage, Amount = 1000m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.AdjustmentIncrease);
        result.Postings.Should().ContainSingle(p => p.AccountId == storage && p.Amount == 1000m);
        result.Postings.Should().NotContain(p => p.AccountId == _systemEndpointId);
    }

    [Fact]
    public async Task CreateShouldCreateAdjustmentDecreaseAndAllowNegativeBalance()
    {
        var userId = Guid.NewGuid();
        var storage = Guid.NewGuid();
        AddEndpoint(storage, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = storage, Amount = -500m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.AdjustmentDecrease);
        result.Postings.Should().ContainSingle(p => p.AccountId == storage && p.Amount == -500m);
    }

    [Fact]
    public async Task CreateShouldReturnAccountNotFoundForSystemAccount()
    {
        var userId = Guid.NewGuid();
        var systemId = Guid.NewGuid();
        AddEndpoint(systemId, userId, EndpointType.System);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = systemId, Amount = 100m }
            ]
        };

        SetupUser(userId);

        var act = () => _transactionService.CreateAsync(model, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<CodedServiceException>();
        ex.Which.ErrorCode.Should().Be("ACCOUNT_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreateShouldReturnPostingsLimitExceeded()
    {
        var userId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings = Enumerable.Range(0, 101)
                .Select(_ => new CreateTransactionPostingModel { AccountId = Guid.NewGuid(), Amount = 1m })
                .ToList()
        };

        SetupUser(userId);

        var act = () => _transactionService.CreateAsync(model, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<CodedServiceException>();
        ex.Which.ErrorCode.Should().Be("POSTINGS_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task CreateShouldReturnDuplicateAccountId()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = accountId, Amount = -100m },
                new CreateTransactionPostingModel { AccountId = accountId, Amount = 100m }
            ]
        };

        SetupUser(userId);

        var act = () => _transactionService.CreateAsync(model, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<CodedServiceException>();
        ex.Which.ErrorCode.Should().Be("DUPLICATE_ACCOUNT_ID");
    }

    [Fact]
    public async Task CreateShouldReturnInvalidPostingCombination()
    {
        var userId = Guid.NewGuid();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        AddEndpoint(a, userId, EndpointType.Storage);
        AddEndpoint(b, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = a, Amount = 100m },
                new CreateTransactionPostingModel { AccountId = b, Amount = -50m }
            ]
        };

        SetupUser(userId);

        var act = () => _transactionService.CreateAsync(model, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<CodedServiceException>();
        ex.Which.ErrorCode.Should().Be("INVALID_POSTING_COMBINATION");
    }

    private void AddEndpoint(Guid id, Guid userId, EndpointType type)
    {
        _endpoints[id] = new TransactionEndpoint
        {
            Id = id,
            UserId = userId,
            EndpointType = type,
            Value = 0m
        };

        _transactionEndpointRepositoryMock
            .Setup(r => r.GetByIdAndUserIdAsync(id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_endpoints[id]);
    }

    private void SetupUser(Guid userId)
    {
        _userRepositoryMock.GetUserByIdReturnNotNull(userId);
        _validateServiceMock
            .Setup(s => s.ValidateAsync(It.IsAny<CreateTransactionModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
```

- [ ] **Step 3: Запустить тесты, убедиться что падают**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter TransactionServiceTests
```

**Ожидаемый результат:**
```
Build failed or tests failed because TransactionService constructor signature mismatch
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/TransactionServiceTests.cs
git commit -m "test: add failing transaction classification tests"
```

### Task 5.2: Реализовать TransactionService

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/TransactionService.cs`
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services.Contracts/ITransactionService.cs`

- [ ] **Step 1: Прочитать оба файла**

- [ ] **Step 2: Заменить содержимое TransactionService.cs**

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

public sealed class TransactionService(
    IUnitOfWork unitOfWork,
    ISmartWalletValidateService validateService,
    IMapper mapper,
    IBackgroundTaskSystemProvider backgroundTaskSystemProvider,
    IOptions<ApiSettings> apiSettings) : ITransactionService
{
    private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
    private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
    private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;
    private readonly IPostingRepository _postingRepository = unitOfWork.PostingRepository;
    private readonly IBackgroundTaskSystemProvider _backgroundTaskSystemProvider = backgroundTaskSystemProvider;
    private readonly ApiSettings _apiSettings = apiSettings.Value;

    async Task<PagedResultModel<TransactionModel>> ITransactionService.GetPagedListByUserIdAsync(
        Guid userId,
        TransactionQueryModel query,
        CancellationToken token)
    {
        await validateService.ValidateAsync(query, token);

        _ = await _userRepository.GetUserByIdAsync(userId, token)
            ?? throw new EntityNotFoundByIdServiceException<User>(userId);

        var dalQuery = mapper.Map<TransactionQuery>(query);
        var pagedResult = await _transactionRepository.GetPagedListByUserIdAsync(userId, dalQuery, token);

        var systemIds = (await _transactionEndpointRepository.GetListByUserIdAsync(userId, token))
            .Where(e => e.EndpointType == EndpointType.System)
            .Select(e => e.Id)
            .ToHashSet();

        var result = mapper.Map<PagedResultModel<TransactionModel>>(pagedResult);
        foreach (var item in result.Items)
        {
            item.Postings.RemoveAll(p => systemIds.Contains(p.AccountId));
        }

        return result;
    }

    async Task<TransactionModel> ITransactionService.CreateAsync(CreateTransactionModel model, CancellationToken token)
    {
        await validateService.ValidateAsync(model, token);

        _ = await _userRepository.GetUserByIdAsync(model.UserId, token)
            ?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);

        var maxPostings = Math.Max(2, _apiSettings.PostingSettings.MaxPostingsPerTransaction);

        ValidatePostings(model.Postings, maxPostings);

        var accountIds = model.Postings.Select(p => p.AccountId).ToList();
        var endpoints = await _transactionEndpointRepository.GetListByIdsAndUserIdAsync(model.UserId, accountIds, token);
        var endpointsById = endpoints.ToDictionary(e => e.Id);

        ValidateAccounts(model.Postings, endpointsById);

        var systemEndpoint = await _transactionEndpointRepository.GetByNameAndUserIdAsync(model.UserId, "System", token)
            ?? throw new CodedServiceException("internal_error", "System endpoint not found", StatusCodes.Status500InternalServerError);

        var (type, systemPosting) = ClassifyTransaction(model.Postings, endpointsById, systemEndpoint.Id);

        var transactionId = Guid.NewGuid();
        var transaction = new Transaction
        {
            Id = transactionId,
            UserId = model.UserId,
            Type = type,
            Postings = BuildPostings(model.Postings, systemPosting, transactionId)
        };

        await ApplyBalanceUpdatesAsync(transaction.Postings, endpointsById, token);

        _transactionRepository.Add(transaction);
        _postingRepository.AddRange(transaction.Postings);
        await unitOfWork.SaveChangesAsync(token);

        var categoryIds = transaction.Postings
            .Where(p => endpointsById.TryGetValue(p.AccountId, out var e) && e.EndpointType == EndpointType.Category)
            .Select(p => p.AccountId)
            .ToHashSet();

        if (categoryIds.Count > 0)
        {
            var day = DateTime.UtcNow.Date;
            _backgroundTaskSystemProvider.FireAndForget<IDailyExpenseCategorieRecalculationService>(s =>
                s.RecalculateManyAsync(model.UserId, categoryIds, day, token));
        }

        var result = mapper.Map<TransactionModel>(transaction);
        result.Postings.RemoveAll(p => p.AccountId == systemEndpoint.Id);
        return result;
    }

    async Task ITransactionService.DeleteAsync(DeleteTransactionModel model, CancellationToken token)
    {
        await validateService.ValidateAsync(model, token);

        if (await _userRepository.GetUserByIdAsync(model.UserId, token) is null)
        {
            throw new EntityNotFoundByIdServiceException<User>(model.UserId);
        }

        var transaction = await _transactionRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
            ?? throw new EntityNotFoundByIdServiceException<Transaction>(model.Id);

        var accountIds = transaction.Postings
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var endpoints = await _transactionEndpointRepository.GetListByIdsAndUserIdAsync(model.UserId, accountIds, token);
        var endpointById = endpoints.ToDictionary(e => e.Id);

        var affectedCategories = new HashSet<Guid>();

        var storageIds = endpoints
            .Where(e => e.EndpointType == EndpointType.Storage)
            .Select(e => e.Id)
            .ToList();

        var categoryIds = endpoints
            .Where(e => e.EndpointType == EndpointType.Category)
            .Select(e => e.Id)
            .ToList();

        var storageBalances = await _transactionRepository.GetStorageBalancesAsync(storageIds, token);
        var categoryBalances = await _transactionRepository.GetCategoryBalancesAsync(categoryIds, token);

        foreach (var posting in transaction.Postings)
        {
            if (!endpointById.TryGetValue(posting.AccountId, out var account))
            {
                continue;
            }

            if (account.EndpointType == EndpointType.System)
            {
                posting.DeletedAt = DateTimeOffset.UtcNow;
                _postingRepository.Update(posting);
                continue;
            }

            var currentBalance = account.EndpointType == EndpointType.Storage
                ? storageBalances.TryGetValue(account.Id, out var sb) ? sb : 0m
                : categoryBalances.TryGetValue(account.Id, out var cb) ? cb : 0m;

            account.Value = currentBalance - posting.Amount;
            _transactionEndpointRepository.Update(account);

            posting.DeletedAt = DateTimeOffset.UtcNow;
            _postingRepository.Update(posting);

            if (account.EndpointType == EndpointType.Category)
            {
                affectedCategories.Add(account.Id);
            }
        }

        _transactionRepository.Delete(transaction);
        await unitOfWork.SaveChangesAsync(token);

        if (affectedCategories.Count > 0)
        {
            var day = transaction.MadeAt.Date;
            _backgroundTaskSystemProvider.FireAndForget<IDailyExpenseCategorieRecalculationService>(s =>
                s.RecalculateManyAsync(model.UserId, affectedCategories, day, token));
        }
    }

    private static void ValidatePostings(List<CreateTransactionPostingModel> postings, int maxPostings)
    {
        if (postings == null || postings.Count == 0)
        {
            throw new CodedServiceException("POSTINGS_EMPTY", "Список проводок пуст", StatusCodes.Status400BadRequest);
        }

        if (postings.Count > maxPostings)
        {
            throw new CodedServiceException("POSTINGS_LIMIT_EXCEEDED",
                $"Превышен лимит проводок ({maxPostings})",
                StatusCodes.Status400BadRequest);
        }

        var seenAccounts = new HashSet<Guid>();

        foreach (var posting in postings)
        {
            if (posting.AccountId == Guid.Empty)
            {
                throw new CodedServiceException("INVALID_ACCOUNT_ID",
                    "Идентификатор счета не может быть пустым",
                    StatusCodes.Status400BadRequest);
            }

            if (posting.Amount == 0)
            {
                throw new CodedServiceException("ZERO_AMOUNT",
                    "Сумма проводки не может быть равна нулю",
                    StatusCodes.Status400BadRequest);
            }

            if (!seenAccounts.Add(posting.AccountId))
            {
                throw new CodedServiceException("DUPLICATE_ACCOUNT_ID",
                    $"Счет {posting.AccountId} указан более одного раза",
                    StatusCodes.Status400BadRequest);
            }
        }
    }

    private static void ValidateAccounts(
        List<CreateTransactionPostingModel> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById)
    {
        foreach (var posting in postings)
        {
            if (!endpointsById.TryGetValue(posting.AccountId, out var endpoint))
            {
                throw new CodedServiceException("ACCOUNT_NOT_FOUND",
                    $"Счет {posting.AccountId} не найден",
                    StatusCodes.Status404NotFound);
            }

            if (endpoint.EndpointType == EndpointType.System)
            {
                throw new CodedServiceException("ACCOUNT_NOT_FOUND",
                    $"Счет {posting.AccountId} не найден",
                    StatusCodes.Status404NotFound);
            }
        }
    }

    private static (TransactionType Type, Posting? SystemPosting) ClassifyTransaction(
        List<CreateTransactionPostingModel> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById,
        Guid systemEndpointId)
    {
        var userSum = postings.Sum(p => p.Amount);
        var storagePostings = postings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Storage)
            .ToList();

        var categoryPostings = postings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Category)
            .ToList();

        if (categoryPostings.Count > 0)
        {
            if (storagePostings.Count > 0
                && storagePostings.All(p => p.Amount < 0)
                && categoryPostings.All(p => p.Amount > 0)
                && userSum == 0)
            {
                return (TransactionType.Expense, null);
            }

            throw new CodedServiceException("INVALID_POSTING_COMBINATION",
                "Комбинация проводок не соответствует ни одному типу транзакции",
                StatusCodes.Status400BadRequest);
        }

        var signs = storagePostings
            .Select(p => Math.Sign(p.Amount))
            .Distinct()
            .ToHashSet();

        if (signs.Count == 2 && userSum == 0)
        {
            return (TransactionType.Transfer, null);
        }

        if (signs.Count == 1 && signs.Contains(1) && userSum > 0)
        {
            return (TransactionType.AdjustmentIncrease, new Posting
            {
                AccountId = systemEndpointId,
                Amount = -userSum
            });
        }

        if (signs.Count == 1 && signs.Contains(-1) && userSum < 0)
        {
            return (TransactionType.AdjustmentDecrease, new Posting
            {
                AccountId = systemEndpointId,
                Amount = -userSum
            });
        }

        throw new CodedServiceException("INVALID_POSTING_COMBINATION",
            "Комбинация проводок не соответствует ни одному типу транзакции",
            StatusCodes.Status400BadRequest);
    }

    private static List<Posting> BuildPostings(
        List<CreateTransactionPostingModel> userPostings,
        Posting? systemPosting,
        Guid transactionId)
    {
        var postings = userPostings
            .Select(p => new Posting
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                AccountId = p.AccountId,
                Amount = p.Amount
            })
            .ToList();

        if (systemPosting != null)
        {
            systemPosting.Id = Guid.NewGuid();
            systemPosting.TransactionId = transactionId;
            postings.Add(systemPosting);
        }

        return postings;
    }

    private async Task ApplyBalanceUpdatesAsync(
        List<Posting> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById,
        CancellationToken token)
    {
        var nonSystemPostings = postings
            .Where(p => endpointsById.TryGetValue(p.AccountId, out var e) && e.EndpointType != EndpointType.System)
            .ToList();

        var storageIds = nonSystemPostings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Storage)
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var categoryIds = nonSystemPostings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Category)
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var storageBalances = await _transactionRepository.GetStorageBalancesAsync(storageIds, token);
        var categoryBalances = await _transactionRepository.GetCategoryBalancesAsync(categoryIds, token);

        foreach (var posting in nonSystemPostings)
        {
            var endpoint = endpointsById[posting.AccountId];

            var currentBalance = endpoint.EndpointType == EndpointType.Storage
                ? storageBalances.TryGetValue(endpoint.Id, out var sb) ? sb : 0m
                : categoryBalances.TryGetValue(endpoint.Id, out var cb) ? cb : 0m;

            endpoint.Value = currentBalance + posting.Amount;
            _transactionEndpointRepository.Update(endpoint);
        }
    }
}
```

- [ ] **Step 3: Обновить ITransactionService.cs**

```csharp
Task<TransactionModel> CreateAsync(CreateTransactionModel model, CancellationToken token);
```

- [ ] **Step 4: Запустить тесты TransactionService**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter TransactionServiceTests
```

**Ожидаемый результат:**
```
Test Run Successful.
```

- [ ] **Step 5: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/TransactionService.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services.Contracts/ITransactionService.cs
git commit -m "feat: implement postings-based transaction classification"
```

---

## Фаза 6. System-счёт при регистрации

### Task 6.1: Написать падающий тест на создание System-счёта

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/UserServiceTests.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Добавить моки и тест**

```csharp
private readonly Mock<ITransactionEndpointRepository> _transactionEndpointRepositoryMock;
private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
```

В конструкторе:
```csharp
_transactionEndpointRepositoryMock = new Mock<ITransactionEndpointRepository>();
_transactionRepositoryMock = new Mock<ITransactionRepository>();

_unitOfWorkMock.Setup(u => u.TransactionEndpointRepository).Returns(_transactionEndpointRepositoryMock.Object);
_unitOfWorkMock.Setup(u => u.TransactionRepository).Returns(_transactionRepositoryMock.Object);
```

Тест:
```csharp
[Fact]
public async Task RegistrationShouldCreateSystemEndpointAlongWithDefaults()
{
    var model = new CreateUserModel
    {
        Email = "test@test.com",
        Password = "password",
        FirstName = "A",
        LastName = "B",
        Patronymic = "C"
    };

    User? capturedUser = null;
    var addedEndpoints = new List<TransactionEndpoint>();

    _validateServiceMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _passwordHasherMock.Setup(p => p.Generate(model.Password)).Returns("hash");
    _userRepositoryMock.Setup(r => r.Add(It.IsAny<User>())).Callback<User>(u => capturedUser = u);
    _transactionEndpointRepositoryMock.Setup(r => r.Add(It.IsAny<TransactionEndpoint>())).Callback<TransactionEndpoint>(e => addedEndpoints.Add(e));
    _mapperMock.Setup(m => m.Map<User>(model)).Returns(new User());
    _mapperMock.Setup(m => m.Map<UserModel>(It.IsAny<User>())).Returns(new UserModel());

    await _userService.RegistrationAsync(model, CancellationToken.None);

    capturedUser.Should().NotBeNull();
    addedEndpoints.Should().ContainSingle(e =>
        e.EndpointType == EndpointType.System &&
        e.Name == "System" &&
        e.Value == 0m &&
        e.Limitation == null &&
        e.UserId == capturedUser!.Id);

    addedEndpoints.Count(e => e.EndpointType == EndpointType.Category).Should().Be(10);
    addedEndpoints.Count(e => e.EndpointType == EndpointType.Storage).Should().Be(2);
}
```

- [ ] **Step 3: Запустить тест, убедиться что падает**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter "RegistrationShouldCreateSystemEndpointAlongWithDefaults"
```

**Ожидаемый результат:**
```
Test failed
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/UserServiceTests.cs
git commit -m "test: add failing test for System endpoint registration"
```

### Task 6.2: Обновить UserService.RegistrationAsync

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/UserService.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Заменить блоки создания категорий/хранилищ и добавить System**

```csharp
foreach (var spendingAreaName in new[] {
    "Продукты", "Кафе и рестораны", "Транспорт",
    "Жилье", "Здоровье", "Одежда и обувь",
    "Развлечения", "Путешествия", "Образование",
    "Подарки"})
{
    _transactionEndpointRepository.Add(new()
    {
        UserId = user.Id,
        Name = spendingAreaName,
        Value = 0.0m,
        EndpointType = EndpointType.Category
    });
}

foreach (var cashVaultName in new[] { "Кошелёк", "Карта" })
{
    _transactionEndpointRepository.Add(new()
    {
        UserId = user.Id,
        Name = cashVaultName,
        Value = 0.0m,
        EndpointType = EndpointType.Storage
    });
}

_transactionEndpointRepository.Add(new()
{
    UserId = user.Id,
    Name = "System",
    Value = 0.0m,
    EndpointType = EndpointType.System,
    Limitation = null
});
```

- [ ] **Step 3: Запустить тест**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter "RegistrationShouldCreateSystemEndpointAlongWithDefaults"
```

**Ожидаемый результат:**
```
Test Run Successful.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/UserService.cs
git commit -m "feat: create System endpoint on user registration"
```

---

## Фаза 7. Публичная фильтрация System

### Task 7.1: Скрыть System из списка конечных точек

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context.Repository/TransactionEndpointRepository.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Добавить фильтр в GetListByUserIdAsync**

```csharp
Task<List<TransactionEndpoint>> ITransactionEndpointRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    => Storage.Read<TransactionEndpoint>()
        .NotDeleted()
        .Where(x => x.UserId == userId)
        .Where(x => x.EndpointType != EndpointType.System)
        .OrderByDescending(x => x.EndpointType)
        .ToListAsync(cancellationToken);
```

### Task 7.2: Дописать тест на фильтрацию System

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Context.Repository.Tests/TransactionEndpointRepositoryTests.cs`

- [ ] **Step 1: Добавить тест**

```csharp
[Fact]
async Task GetListByUserIdShouldNotReturnSystemEndpoint()
{
    var userId = Guid.NewGuid();

    var transactionEndpoints = new List<TransactionEndpoint>
    {
        _entityProvider.Create<TransactionEndpoint>(x => {
            x.UserId = userId;
            x.EndpointType = EndpointType.System;
            x.Name = "System";
        }),
        _entityProvider.Create<TransactionEndpoint>(x => {
            x.UserId = userId;
            x.EndpointType = EndpointType.Storage;
        })
    };

    await Context.AddRangeAsync(transactionEndpoints);
    await Context.SaveChangesAsync();

    var result = await _transactionEndpointRepository.GetListByUserIdAsync(userId, CancellationToken.None);

    result.Should().HaveCount(1);
    result.Should().ContainSingle(x => x.EndpointType == EndpointType.Storage);
}
```

- [ ] **Step 3: Запустить тесты**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Context.Repository.Tests/Context.Repository.Tests.csproj --filter TransactionEndpointRepositoryTests
```

**Ожидаемый результат:**
```
Test Run Successful.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context.Repository/TransactionEndpointRepository.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Context.Repository.Tests/TransactionEndpointRepositoryTests.cs
git commit -m "feat: hide System endpoints from public list"
```

### Task 7.3: Убедиться, что System-постинги скрыты в ответе

- [ ] **Step 1: Запустить тест**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Service.Tests/Service.Tests.csproj --filter "CreateShouldCreateAdjustmentIncreaseAndHideSystemPosting"
```

**Ожидаемый результат:**
```
Test Run Successful.
```

- [ ] **Step 2: Коммит (если меняли mapper)**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/Services/AutoMappers/ServiceModelMapper.cs
git commit -m "feat: filter System postings from public transaction models"
```

---

## Фаза 8. Обновление TransactionController

### Task 8.1: Вернуть 201 Created и ApiErrorApiModel

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Controllers/TransactionController.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: Изменить метод Create и атрибуты**

```csharp
[HttpPost]
[ProducesResponseType(typeof(TransactionApiModel), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ApiErrorApiModel), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiErrorApiModel), StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> Create([FromBody] CreateTransactionApiModel request, CancellationToken token)
{
    var model = _mapper.Map<CreateTransactionModel>(request);
    model.UserId = _identityProvider.Id;
    var response = await _transactionService.CreateAsync(model, token);
    var apiModel = _mapper.Map<TransactionApiModel>(response);
    return StatusCode(StatusCodes.Status201Created, apiModel);
}
```

- [ ] **Step 3: Собрать проект SmartWallet**

**Команда:**
```bash
dotnet build /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/SmartWallet.csproj
```

**Ожидаемый результат:**
```
Build succeeded.
```

- [ ] **Step 4: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/Controllers/TransactionController.cs
git commit -m "feat: return 201 Created and ApiErrorApiModel from transaction controller"
```

---

## Фаза 9. Миграция и пересоздание локальной БД

### Task 9.1: Установить/проверить EF CLI

- [ ] **Step 1: Проверить версию EF CLI**

**Команда:**
```bash
dotnet ef --version
```

**Ожидаемый результат:**
```
8.0.x
```

Если не установлен:
```bash
dotnet tool install --global dotnet-ef
```

### Task 9.2: Сгенерировать временную миграцию для обновления снапшота

- [ ] **Step 1: Запустить команду**

**Команда:**
```bash
dotnet ef migrations add EndpointTypeSchema \
  --project /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context/Context.csproj \
  --startup-project /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/SmartWallet.csproj
```

**Ожидаемый результат:**
```
Build started...
Build succeeded.
Done.
```

### Task 9.3: Внести операции EndpointType в существующую миграцию

**Files:**
- Modify: `/mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context/Migrations/20260726202412_DbSchemaUpdate.cs`

- [ ] **Step 1: Прочитать текущий файл**

- [ ] **Step 2: В метод Up добавить**

```csharp
migrationBuilder.DropColumn(
    name: "IsStorage",
    table: "TransactionEndpoint");

migrationBuilder.AddColumn<int>(
    name: "EndpointType",
    table: "TransactionEndpoint",
    type: "integer",
    nullable: false,
    defaultValue: 0);
```

- [ ] **Step 3: В метод Down добавить**

```csharp
migrationBuilder.DropColumn(
    name: "EndpointType",
    table: "TransactionEndpoint");

migrationBuilder.AddColumn<bool>(
    name: "IsStorage",
    table: "TransactionEndpoint",
    type: "boolean",
    nullable: false,
    defaultValue: false);
```

### Task 9.4: Удалить временные файлы миграции

- [ ] **Step 1: Удалить временные файлы**

**Команда:**
```bash
rm /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context/Migrations/*EndpointTypeSchema.cs \
   /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context/Migrations/*EndpointTypeSchema.Designer.cs
```

**Важно:** не запускать `dotnet ef migrations remove`, чтобы сохранить обновлённый `SmartWalletContextModelSnapshot.cs`.

### Task 9.5: Удалить и пересоздать локальную БД

- [ ] **Step 1: Пересоздать БД**

**Команды:**
```bash
dropdb -U postgres SmartWalletDb
createdb -U postgres SmartWalletDb
```

Если недоступны:
```bash
psql -U postgres -c "DROP DATABASE IF EXISTS \"SmartWalletDb\";"
psql -U postgres -c "CREATE DATABASE \"SmartWalletDb\";"
```

**Ожидаемый результат:**
```
CREATE DATABASE
```

### Task 9.6: Применить миграции

- [ ] **Step 1: Применить миграции**

**Команда:**
```bash
dotnet ef database update \
  --project /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context/Context.csproj \
  --startup-project /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/SmartWallet.csproj
```

**Ожидаемый результат:**
```
Applying migration '...'.
Done.
```

- [ ] **Step 2: Коммит**

```bash
git add /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context/Migrations/20260726202412_DbSchemaUpdate.cs \
        /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/DAL/Context/Migrations/SmartWalletContextModelSnapshot.cs
git commit -m "feat: update local migration and snapshot for EndpointType"
```

---

## Фаза 10. Итоговая проверка

### Task 10.1: Запустить все тесты

- [ ] **Step 1: Запустить все тесты**

**Команда:**
```bash
dotnet test /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet.sln
```

**Ожидаемый результат:**
```
Test Run Successful.
Total tests: ...
```

### Task 10.2: Запустить приложение

- [ ] **Step 1: Запустить приложение**

**Команда:**
```bash
dotnet run --project /mnt/c/ProgramProjects/SmartWalletApp/SmartWallet/SmartWallet/SmartWallet.csproj
```

### Task 10.3: Проверить сценарии через curl

- [ ] **Step 1: Регистрация**

```bash
curl -X POST http://localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"email":"u1@test.com","password":"pass123","firstName":"A","lastName":"B","patronymic":"C"}'
```

- [ ] **Step 2: Авторизация**

```bash
curl -X POST http://localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"u1@test.com","password":"pass123"}'
```

- [ ] **Step 3: Transfer**

```bash
curl -X POST http://localhost:5000/api/transactions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"postings":[{"accountId":"<storage-a>","amount":-500},{"accountId":"<storage-b>","amount":500}]}'
```

**Ожидаемый результат:** HTTP 201, `type: "Transfer"`, 2 postings.

- [ ] **Step 4: Adjustment**

```bash
curl -X POST http://localhost:5000/api/transactions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"postings":[{"accountId":"<storage>","amount":1000}]}'
```

**Ожидаемый результат:** HTTP 201, `type: "AdjustmentIncrease"`, 1 posting.

- [ ] **Step 5: Превышение лимита**

```bash
curl -X POST http://localhost:5000/api/transactions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"postings":[<101 проводок>]}'
```

**Ожидаемый результат:** HTTP 400, `{"code":"POSTINGS_LIMIT_EXCEEDED","message":"..."}`.

### Task 10.4: Финальный коммит

- [ ] **Step 1: Закоммитить оставшиеся изменения**

```bash
git add .
git commit -m "feat: complete postings-based transaction implementation"
```

---

## Примечания

- Файл `TransactionType.cs` не модифицировать.
- `EndpointType` сериализуется в API как строка благодаря глобальному `JsonStringEnumConverter`.
- System-счёт, переданный клиентом в postings, возвращает `404 ACCOUNT_NOT_FOUND`.
- Отрицательный баланс Storage допустим; проверка Limitation для транзакций не реализовывается.
- Миграция применяется только к локальной БД; production-миграция не создаётся.
- Все операции создания транзакции выполняются в одном `SaveChangesAsync` EF Core.
