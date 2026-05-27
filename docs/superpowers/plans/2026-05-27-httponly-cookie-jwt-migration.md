# HttpOnly Cookie JWT Migration — Implementation Plan

## Goal

Migrate JWT authentication from client-side token storage (localStorage + Authorization header) to a dual-token architecture:
- **Access token** (JWT, 15 min): returned in response body, sent via `Authorization: Bearer` header
- **Refresh token** (Guid, 7 days): stored in httpOnly cookie `"refresh_token"`, sent automatically by browser

This eliminates XSS vulnerability from localStorage while maintaining stateless JWT validation for access tokens.

## Architecture

- **Data layer**: New `RefreshToken` entity with EF Core configuration, `IRefreshTokenRepository`, `RefreshTokenRepository`, updated `IUnitOfWork`/`UnitOfWork`
- **Service layer**: Updated `IUserService`/`UserService` with tuple return from `LogInAsync`, new `RefreshAsync`/`LogoutAsync` methods
- **API layer**: Updated `UserController` with cookie management, new `Refresh`/`LogOut` endpoints, new response models
- **Configuration**: `JwtOptions` renamed property + new property, `JwtProvider` uses minutes, `Program.cs` removes `AddCookie()`, adds DI registrations

## Tech Stack

- .NET 8, C# 12
- EF Core (PostgreSQL/Npgsql)
- xUnit, Moq, FluentAssertions
- AutoMapper

## File Structure

### New Files

| File | Purpose |
|------|---------|
| `DAL/Entities/RefreshToken.cs` | RefreshToken entity |
| `DAL/Entity.Configuration/RefreshTokenConfiguration.cs` | EF Core entity configuration |
| `DAL/Context.Repository.Contracts/IRefreshTokenRepository.cs` | Repository interface |
| `DAL/Context.Repository/RefreshTokenRepository.cs` | Repository implementation |
| `SmartWallet/Models/Account/ResponseRefreshApiModel.cs` | Refresh response model |
| `Service.Tests/RefreshTokenRepositoryTests.cs` | Not needed (integration); unit tests go in `Service.Tests/UserServiceTests.cs` |

### Modified Files

| File | Change |
|------|--------|
| `DAL/Entities/User.cs` | Add `RefreshTokens` navigation property |
| `DAL/Context.Repository.Contracts/IUnitOfWork.cs` | Add `RefreshTokenRepository` property |
| `DAL/Context.Repository/UnitOfWork.cs` | Add `RefreshTokenRepository` instantiation |
| `DAL/Context/SmartWalletContext.cs` | Add `DbSet<RefreshToken>` |
| `SmartWallet.Options/JwtOptions.cs` | Rename `ExpiresHours` → `ExpiresMinutes`, add `RefreshExpiresDays` |
| `Services.Infrastructure/JwtProvider.cs` | Change `AddHours` → `AddMinutes` |
| `Services.Contracts/IUserService.cs` | Update `LogInAsync` return type, add `RefreshAsync`/`LogoutAsync` |
| `Services/UserService.cs` | Implement new methods, update `LogInAsync` |
| `SmartWallet/Controllers/UserController.cs` | Rewrite `LogIn`, add `Refresh`/`LogOut` endpoints |
| `SmartWallet/Models/Account/ResponseLogInApiModel.cs` | Replace `JwtToken` with `AccessToken` |
| `SmartWallet/Program.cs` | Remove `AddCookie()`, add `IRefreshTokenRepository` DI, update auth |
| `SmartWallet/appsettings.json` | Update `JwtSettings` section |
| `Service.Tests/UserServiceTests.cs` | Update `LogInAsync` tests, add `RefreshAsync`/`LogoutAsync` tests |
| `UnitTests.Services.Infrastructure/MockedUnitOfWork.cs` | Add `RefreshTokenRepository` mock |

---

## Task 1: Create RefreshToken Entity

**Estimated time**: 3 min

Create the `RefreshToken` entity in the DAL/Entities project.

```csharp
// DAL/Entities/RefreshToken.cs
namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Рефреш-токен
/// </summary>
public class RefreshToken : BaseEntity
{
	/// <summary>
	/// Значение токена
	/// </summary>
	public string Token { get; set; }

	/// <summary>
	/// Идентификатор пользователя (FK)
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Дата истечения срока действия
	/// </summary>
	public DateTime ExpiresAt { get; set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Дата отзыва (null = активный)
	/// </summary>
	public DateTime? RevokedAt { get; set; }

	/// <summary>
	/// Токен, заменивший данный
	/// </summary>
	public string? ReplacedByToken { get; set; }

	/// <summary>
	/// Навигационное свойство к пользователю
	/// </summary>
	public User User { get; set; }
}
```

**Commit**: `feat: add RefreshToken entity`

---

## Task 2: Add RefreshTokens Navigation Property to User Entity

**Estimated time**: 2 min

Add the `RefreshTokens` collection to `User.cs`.

```csharp
// DAL/Entities/User.cs — full file after change
namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class User : SmartDeletedEntity
{
	/// <summary>
	/// Электронная почта
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	/// Имя
	/// </summary>
	public string FirstName { get; set; }
		
	/// <summary>
	/// Фамилия
	/// </summary>
	public string LastName { get; set; }

	/// <summary>
	/// Отчество
	/// </summary>
	public string Patronymic { get; set; }

	/// <summary>
	/// Хешированный пароль
	/// </summary>
	public string HashedPassword { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public ICollection<Transaction> Transactions { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public ICollection<TransactionEndpoint> TransactionEndpoints { get; set; }

	/// <summary>
	/// Навигационное свойство — рефреш-токены пользователя
	/// </summary>
	public ICollection<RefreshToken> RefreshTokens { get; set; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="User"/> 
	/// </summary>
	public User()
	{
		Transactions = new List<Transaction>();
		TransactionEndpoints = new List<TransactionEndpoint>();
		RefreshTokens = new List<RefreshToken>();
	}
}
```

**Commit**: `feat: add RefreshTokens navigation property to User entity`

---

## Task 3: Create RefreshToken EF Core Configuration

**Estimated time**: 3 min

Create the entity configuration for `RefreshToken` following the existing pattern.

```csharp
// DAL/Entity.Configuration/RefreshTokenConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="RefreshToken"/>
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<RefreshToken> builder)
	{
		builder.ToTable(nameof(RefreshToken));

		builder.HasKey(x => x.Id);

		builder.HasIndex(x => x.Token).IsUnique();

		builder.HasIndex(x => x.UserId);

		builder.Property(x => x.Token).IsRequired();

		builder.HasOne(x => x.User)
			.WithMany(x => x.RefreshTokens)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
```

**Commit**: `feat: add RefreshToken EF Core configuration`

---

## Task 4: Add DbSet<RefreshToken> to SmartWalletContext

**Estimated time**: 2 min

Add the `DbSet<RefreshToken>` property to the context.

```csharp
// DAL/Context/SmartWalletContext.cs — full file after change
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Entity.Configuration;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context;

/// <summary>
/// Контекст работы с базой данных
/// </summary>
public class SmartWalletContext : DbContext, IDataStorageContext
{
	/// <summary>
	/// Рефреш-токены
	/// </summary>
	public DbSet<RefreshToken> RefreshTokens { get; set; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletContext"/>
	/// </summary>
	public SmartWalletContext(DbContextOptions<SmartWalletContext> options)
		: base(options)
	{
	}

	IQueryable<TEntity> IDataStorageContext.Read<TEntity>() where TEntity : class 
		=> Set<TEntity>().AsNoTracking().AsQueryable();

	void IDataStorageContext.Create<TEntity>(TEntity entity) where TEntity : class 
		=> Entry(entity).State = EntityState.Added;

	void IDataStorageContext.Delete<TEntity>(TEntity entity) where TEntity : class 
		=> Entry(entity).State = EntityState.Deleted;

	void IDataStorageContext.Update<TEntity>(TEntity entity) where TEntity : class
		=> Entry(entity).State = EntityState.Modified;

	/// <inheritdoc />
	async Task<int> IDataStorageContext.SaveChangesAsync(CancellationToken cancellationToken)
	{
		var count = await base.SaveChangesAsync(cancellationToken);

		foreach (var entry in base.ChangeTracker.Entries().ToArray())
		{
			entry.State = EntityState.Unchanged;
		}
		
		return count;
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartWalletAnchorEntity).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
```

**Commit**: `feat: add RefreshTokens DbSet to SmartWalletContext`

---

## Task 5: Create IRefreshTokenRepository Interface

**Estimated time**: 3 min

Create the repository interface following the existing pattern.

```csharp
// DAL/Context.Repository.Contracts/IRefreshTokenRepository.cs
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts;

/// <summary>
/// Репозиторий для работы с <see cref="RefreshToken"/>
/// </summary>
public interface IRefreshTokenRepository : IBaseWriteRepository<RefreshToken>
{
	/// <summary>
	/// Возвращает рефреш-токен по значению токена
	/// </summary>
	Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

	/// <summary>
	/// Возвращает все активные рефреш-токены пользователя
	/// </summary>
	Task<IReadOnlyCollection<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
```

**Commit**: `feat: add IRefreshTokenRepository interface`

---

## Task 6: Create RefreshTokenRepository Implementation

**Estimated time**: 3 min

Create the repository implementation following the existing pattern.

```csharp
// DAL/Context.Repository/RefreshTokenRepository.cs
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="RefreshToken"/>
/// </summary>
public class RefreshTokenRepository(IDataStorageContext storage) : BaseWriteRepository<RefreshToken>(storage), IRefreshTokenRepository
{
	Task<RefreshToken?> IRefreshTokenRepository.GetByTokenAsync(string token, CancellationToken cancellationToken)
		=> Storage.Read<RefreshToken>().FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

	Task<IReadOnlyCollection<RefreshToken>> IRefreshTokenRepository.GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> Storage.Read<RefreshToken>()
			.Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow)
			.ToListAsync(cancellationToken);
}
```

**Commit**: `feat: add RefreshTokenRepository implementation`

---

## Task 7: Update IUnitOfWork — Add RefreshTokenRepository

**Estimated time**: 2 min

Add the `RefreshTokenRepository` property to `IUnitOfWork`.

```csharp
// DAL/Context.Repository.Contracts/IUnitOfWork.cs — full file after change
namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Интерфейс UnitOfWork
	/// </summary>
	public interface IUnitOfWork
	{
		/// <inheritdoc cref="TransactionEndpointRepository"/>
		ITransactionEndpointRepository TransactionEndpointRepository { get; }

		/// <inheritdoc cref="ITransactionRepository"/>
		ITransactionRepository TransactionRepository { get; }

		/// <inheritdoc cref="IUserRepository"/>
		IUserRepository UserRepository { get; }

		/// <inheritdoc cref="IRefreshTokenRepository"/>
		IRefreshTokenRepository RefreshTokenRepository { get; }

		/// <summary>
		/// Сохраняет изменения
		/// </summary>
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}
```

**Commit**: `feat: add RefreshTokenRepository to IUnitOfWork`

---

## Task 8: Update UnitOfWork — Add RefreshTokenRepository

**Estimated time**: 2 min

Add the `RefreshTokenRepository` instantiation to `UnitOfWork`.

```csharp
// DAL/Context.Repository/UnitOfWork.cs — full file after change
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Паттерн единица работы
/// </summary>
public class UnitOfWork : IUnitOfWork
{
	private readonly IDataStorageContext storage;

	public IUserRepository UserRepository { get; init; }

	public ITransactionEndpointRepository TransactionEndpointRepository { get; init; }

	public ITransactionRepository TransactionRepository { get; init; }

	public IRefreshTokenRepository RefreshTokenRepository { get; init; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UnitOfWork"/>
	/// </summary>
	public UnitOfWork(IDataStorageContext storage)
	{
		this.storage =  storage;

		UserRepository = new UserRepository(storage);
		TransactionEndpointRepository = new TransactionEndpointRepository(storage);
		TransactionRepository = new TransactionRepository(storage);
		RefreshTokenRepository = new RefreshTokenRepository(storage);
	}

	Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
		=> storage.SaveChangesAsync(cancellationToken);
}
```

**Commit**: `feat: add RefreshTokenRepository to UnitOfWork`

---

## Task 9: Update JwtOptions — Rename ExpiresHours to ExpiresMinutes, Add RefreshExpiresDays

**Estimated time**: 2 min

```csharp
// SmartWallet.Options/JwtOptions.cs — full file after change
namespace Nasurino.SmartWallet.Options;

/// <summary>
/// Настройки конфигурации для Jwt
/// </summary>
public class JwtOptions
{
	/// <summary>
	/// Ключ для генерации Jwt
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Количество минут, которое действует access-токен
	/// </summary>
	public int ExpiresMinutes { get; set; }

	/// <summary>
	/// Количество дней, которое действует refresh-токен
	/// </summary>
	public int RefreshExpiresDays { get; set; }
}
```

**Commit**: `feat: update JwtOptions — ExpiresMinutes + RefreshExpiresDays`

---

## Task 10: Update JwtProvider — AddMinutes Instead of AddHours

**Estimated time**: 2 min

```csharp
// Services.Infrastructure/JwtProvider.cs — full file after change
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Models.Models;
using Service.Infrastructure.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Nasurino.SmartWallet.Service.Infrastructure;

/// <summary>
/// Провайдер JWT
/// </summary>
public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
	private readonly JwtOptions options = options.Value;

	string IJwtProvider.GenerateToken(UserModel user)
	{
		var signingCredentials = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
			SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
			new (JwtRegisteredClaimNames.NameId, user.Id.ToString()),
		};

		var token = new JwtSecurityToken(
			claims: claims,
			signingCredentials: signingCredentials,
			expires: DateTime.UtcNow.AddMinutes(options.ExpiresMinutes));

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
```

**Commit**: `feat: update JwtProvider to use ExpiresMinutes`

---

## Task 11: Update appsettings.json — JwtSettings Section

**Estimated time**: 2 min

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "SmartWalletConnectionString": "",
    "HangfireConnection": ""
  },

  "ApiSettings": {
    "JwtSettings": {
      "Key": "EA!=g231sadvAerFargad$@!@!*(rgBgszgedJKL1231!",
      "ExpiresMinutes": "15",
      "RefreshExpiresDays": "7"
    },

    "BCryptSettings": {
      "WorkFactor": "12"
    },

    "CqrsSettings": {
      "AllowedOrigins" : [
        "http://localhost:5173"
      ]
    }
  }
}
```

**Commit**: `feat: update appsettings.json — ExpiresMinutes + RefreshExpiresDays`

---

## Task 12: Update IUserService Interface

**Estimated time**: 3 min

Change `LogInAsync` return type and add `RefreshAsync`/`LogoutAsync`.

```csharp
// Services.Contracts/IUserService.cs — full file after change
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса для работы с пользователем
	/// </summary>
	public interface IUserService
	{
		/// <summary>
		/// Удаление пользователя
		/// </summary>
		Task DeleteAsync(DeleteUserModel model, CancellationToken token);

		/// <summary>
		/// Возвращает пользователя по Id
		/// </summary>
		Task<UserModel> GetUserByIdAsync(Guid userId, CancellationToken token);

		/// <summary>
		/// Вход в аккаунт. Возвращает access-токен и refresh-токен
		/// </summary>
		Task<(string AccessToken, string RefreshToken)> LogInAsync(LogInModel model, CancellationToken token);

		/// <summary>
		/// Обновление access-токена по refresh-токену. Возвращает новый access-токен и новый refresh-токен
		/// </summary>
		Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken, CancellationToken token);

		/// <summary>
		/// Выход из аккаунта — отзыв refresh-токена
		/// </summary>
		Task LogoutAsync(string refreshToken, CancellationToken token);

		/// <summary>
		/// Регистрация
		/// </summary>
		Task<UserModel> RegistrationAsync(CreateUserModel model, CancellationToken token);

		/// <summary>
		/// Обновление пользователя
		/// </summary>
		Task<UserModel> UpdateAsync(UpdateUserModel model, CancellationToken token);

		/// <summary>
		/// Смена пароля пользователя
		/// </summary>
		Task ChangePasswordAsync(ChangePasswordModel model, CancellationToken token);
	}
}
```

**Commit**: `feat: update IUserService — LogInAsync returns tuple, add RefreshAsync/LogoutAsync`

---

## Task 13: Update UserService — Implement LogInAsync, RefreshAsync, LogoutAsync

**Estimated time**: 5 min

This is the core service logic change. The `UserService` constructor now needs `JwtOptions` for refresh token expiration, and the `IRefreshTokenRepository` from `IUnitOfWork`.

```csharp
// Services/UserService.cs — full file after change
using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Service.Infrastructure.Contracts;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с пользователем
/// </summary>
public sealed class UserService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IPasswordHasher passwordHasher,
	IJwtProvider jwtProvider,
	IMapper mapper,
	JwtOptions jwtOptions) : IUserService
{
	private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
	private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;
	private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository = unitOfWork.RefreshTokenRepository;

	async Task<UserModel> IUserService.GetUserByIdAsync(Guid userId, CancellationToken token)
	{
		var user = await _userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundByIdServiceException<User>(userId);

		return mapper.Map<UserModel>(user);
	}

	async Task<UserModel> IUserService.RegistrationAsync(CreateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = mapper.Map<User>(model);
		user.Id = Guid.NewGuid();
		user.HashedPassword = passwordHasher.Generate(model.Password);
		_userRepository.Add(user);

		//Создание базовых областей трат для пользователя
		foreach (var spendingAreaName in new[] {
			"Продукты", "Кафе и рестораны","Транспорт",
			"Жилье", "Здоровье", "Одежда и обувь",
			"Развлечения", "Путешествия", "Образование",
			"Подарки"})
		{
			_transactionEndpointRepository.Add(
				new()
				{
					UserId = user.Id,
					Name = spendingAreaName,
					Value = 0.0,
					IsStorage = false
				});
		}

		//Создание базовых денежных хранилищ для пользователя
		foreach (var cashVaultName in new[] { "Кошелёк", "Карта" })
		{
			_transactionEndpointRepository.Add(new()
			{
				UserId = user.Id,
				Name = cashVaultName,
				Value = 0.0,
				IsStorage = true
			});
		}
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<UserModel>(user);
	}

	async Task<(string AccessToken, string RefreshToken)> IUserService.LogInAsync(LogInModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = await _userRepository.GetUserByEmailAsync(model.Email, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с адресом электронной почты = {model.Email} не найден.");
		if (!passwordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthenticationServiceException();
		}

		var accessToken = jwtProvider.GenerateToken(mapper.Map<UserModel>(user));
		var refreshTokenValue = Guid.NewGuid().ToString();
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Token = refreshTokenValue,
			UserId = user.Id,
			ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.RefreshExpiresDays),
			CreatedAt = DateTime.UtcNow,
		};
		_refreshTokenRepository.Add(refreshToken);
		await unitOfWork.SaveChangesAsync(token);

		return (accessToken, refreshTokenValue);
	}

	async Task<(string AccessToken, string RefreshToken)> IUserService.RefreshAsync(string refreshTokenValue, CancellationToken token)
	{
		var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, token)
			?? throw new AuthenticationServiceException();

		if (storedToken.ExpiresAt < DateTime.UtcNow)
		{
			throw new AuthenticationServiceException();
		}

		if (storedToken.RevokedAt is not null)
		{
			throw new AuthenticationServiceException();
		}

		var user = await _userRepository.GetUserByIdAsync(storedToken.UserId, token)
			?? throw new AuthenticationServiceException();

		// Mark old refresh token as revoked
		storedToken.RevokedAt = DateTime.UtcNow;

		// Generate new tokens
		var newAccessToken = jwtProvider.GenerateToken(mapper.Map<UserModel>(user));
		var newRefreshTokenValue = Guid.NewGuid().ToString();
		var newRefreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Token = newRefreshTokenValue,
			UserId = user.Id,
			ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.RefreshExpiresDays),
			CreatedAt = DateTime.UtcNow,
		};

		storedToken.ReplacedByToken = newRefreshTokenValue;
		_refreshTokenRepository.Update(storedToken);
		_refreshTokenRepository.Add(newRefreshToken);
		await unitOfWork.SaveChangesAsync(token);

		return (newAccessToken, newRefreshTokenValue);
	}

	async Task IUserService.LogoutAsync(string refreshTokenValue, CancellationToken token)
	{
		var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, token);
		if (storedToken is not null)
		{
			storedToken.RevokedAt = DateTime.UtcNow;
			_refreshTokenRepository.Update(storedToken);
			await unitOfWork.SaveChangesAsync(token);
		}
	}

	async Task<UserModel> IUserService.UpdateAsync(UpdateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await _userRepository.GetUserByIdAsync(model.Id, token)
			?? throw new EntityNotFoundByIdServiceException<User>(model.Id);
		mapper.Map(model, user);
		_userRepository.Update(user);
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<UserModel>(user);
	}

	async Task IUserService.DeleteAsync(DeleteUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await _userRepository.GetUserByIdAsync(model.Id, token)
			?? throw new EntityNotFoundByIdServiceException<User>(model.Id);

		if (!passwordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthenticationServiceException();
		}
		_userRepository.Delete(user);
		_transactionEndpointRepository.DeleteTransactionEndpointsByUserId(user.Id);
		_transactionRepository.DeleteTransactionsByUserId(user.Id);

		await unitOfWork.SaveChangesAsync(token);
	}

	async Task IUserService.ChangePasswordAsync(ChangePasswordModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await _userRepository.GetUserByIdAsync(model.UserId, token)
			?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);

		if (!passwordHasher.Verify(model.OldPassword, user.HashedPassword))
		{
			throw new AuthenticationServiceException();
		}

		user.HashedPassword = passwordHasher.Generate(model.NewPassword);
		_userRepository.Update(user);
		await unitOfWork.SaveChangesAsync(token);
	}
}
```

**Commit**: `feat: implement LogInAsync tuple return, RefreshAsync, LogoutAsync in UserService`

---

## Task 14: Update ResponseLogInApiModel — Replace JwtToken with AccessToken

**Estimated time**: 2 min

```csharp
// SmartWallet/Models/Account/ResponseLogInApiModel.cs — full file after change
namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель ответа входа пользователя
/// </summary>
public class ResponseLogInApiModel
{
	/// <summary>
	/// Access-токен (JWT)
	/// </summary>
	public string AccessToken { get; set; }
}
```

**Commit**: `feat: update ResponseLogInApiModel — JwtToken → AccessToken`

---

## Task 15: Create ResponseRefreshApiModel

**Estimated time**: 2 min

```csharp
// SmartWallet/Models/Account/ResponseRefreshApiModel.cs
namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель ответа обновления токена
/// </summary>
public class ResponseRefreshApiModel
{
	/// <summary>
	/// Access-токен (JWT)
	/// </summary>
	public string AccessToken { get; set; }
}
```

**Commit**: `feat: add ResponseRefreshApiModel`

---

## Task 16: Update UserController — Rewrite LogIn, Add Refresh and LogOut

**Estimated time**: 5 min

This is the main controller change. Remove `IWebHostEnvironment` and `JwtOptions` dependencies (no longer needed for cookie expiry — we use `JwtOptions` via `IOptions<JwtOptions>` for refresh cookie expiry). Actually, we still need `JwtOptions` for the refresh cookie `Expires` and `IWebHostEnvironment` for `IsProduction()` check on the `Secure` flag.

```csharp
// SmartWallet/Controllers/UserController.cs — full file after change
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.Account;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с пользователем
/// </summary>
[Route("api/[controller]")]
[ApiController]
public sealed class UserController : Controller
{
	private readonly IUserService _userService;
	private readonly IIdentityProvider _identityProvider;
	private readonly IWebHostEnvironment _environment;
	private readonly JwtOptions _jwtOptions;
	private readonly IMapper _mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UserController"/>
	/// </summary>
	public UserController(IUserService userService,
		IIdentityProvider identityProvider,
		IWebHostEnvironment environment,
		JwtOptions jwtOptions,
		IMapper mapper)
	{
		_userService = userService;
		_identityProvider = identityProvider;
		_environment = environment;
		_jwtOptions = jwtOptions;
		_mapper = mapper;
	}

	/// <summary>
	/// Получает данные о пользователе
	/// </summary>
	[HttpGet]
	[Authorize]
	[ProducesResponseType(typeof(UserApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Get(CancellationToken token)
	{
		var responce = await _userService.GetUserByIdAsync(_identityProvider.Id, token);
		return Ok(_mapper.Map<UserApiModel>(responce));
	}

	/// <summary>
	/// Регистрирует пользователя
	/// </summary>
	[HttpPost]
	[AllowAnonymous]
	[ProducesResponseType(typeof(UserApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> SignIn([FromBody] CreateUserApiModel request, CancellationToken token)
	{
		var response = await _userService.RegistrationAsync(_mapper.Map<CreateUserModel>(request), token);
		return Ok(_mapper.Map<UserApiModel>(response));
	}

	/// <summary>
	/// Вход в аккаунт
	/// </summary>
	[HttpPut("login")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(ResponseLogInApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> LogIn([FromBody] RequestLogInApiModel request, CancellationToken token)
	{
		var (accessToken, refreshToken) = await _userService.LogInAsync(_mapper.Map<LogInModel>(request), token);

		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = _environment.IsProduction(),
			SameSite = SameSiteMode.Lax,
			Path = "/",
			Expires = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshExpiresDays)
		};

		Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
		return Ok(new ResponseLogInApiModel { AccessToken = accessToken });
	}

	/// <summary>
	/// Обновление access-токена
	/// </summary>
	[HttpPost("refresh")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(ResponseRefreshApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Refresh(CancellationToken token)
	{
		var refreshToken = Request.Cookies["refresh_token"];
		if (string.IsNullOrEmpty(refreshToken))
		{
			return Unauthorized();
		}

		try
		{
			var (accessToken, newRefreshToken) = await _userService.RefreshAsync(refreshToken, token);

			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = _environment.IsProduction(),
				SameSite = SameSiteMode.Lax,
				Path = "/",
				Expires = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshExpiresDays)
			};

			Response.Cookies.Append("refresh_token", newRefreshToken, cookieOptions);
			return Ok(new ResponseRefreshApiModel { AccessToken = accessToken });
		}
		catch (AuthenticationServiceException)
		{
			DeleteRefreshTokenCookie();
			throw;
		}
	}

	/// <summary>
	/// Выход из аккаунта
	/// </summary>
	[HttpPost("logout")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> LogOut(CancellationToken token)
	{
		var refreshToken = Request.Cookies["refresh_token"];
		if (!string.IsNullOrEmpty(refreshToken))
		{
			await _userService.LogoutAsync(refreshToken, token);
		}

		DeleteRefreshTokenCookie();
		return Ok();
	}

	/// <summary>
	/// Обновление пользователя
	/// </summary>
	[HttpPut]
	[Authorize]
	[ProducesResponseType(typeof(UserApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Update([FromBody] UpdateUserApiModel request, CancellationToken token)
	{
		var updateModel = _mapper.Map<UpdateUserModel>(request);
		updateModel.Id = _identityProvider.Id;
		var response = await _userService.UpdateAsync(updateModel, token);
		return Ok(_mapper.Map<UserApiModel>(response));
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	[HttpDelete]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] DeleteUserApiModel request, CancellationToken token)
	{
		var updateModel = _mapper.Map<DeleteUserModel>(request);
		updateModel.Id = _identityProvider.Id;
		await _userService.DeleteAsync(updateModel, token);
		return Ok();
	}

	/// <summary>
	/// Смена пароля пользователя
	/// </summary>
	[HttpPut("password")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordApiModel request, CancellationToken token)
	{
		var model = _mapper.Map<ChangePasswordModel>(request);
		model.UserId = _identityProvider.Id;
		await _userService.ChangePasswordAsync(model, token);
		return Ok();
	}

	private void DeleteRefreshTokenCookie()
	{
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = _environment.IsProduction(),
			SameSite = SameSiteMode.Lax,
			Path = "/",
			Expires = DateTimeOffset.UtcNow.AddDays(-1)
		};

		Response.Cookies.Append("refresh_token", "", cookieOptions);
	}
}
```

**Commit**: `feat: rewrite UserController — LogIn with cookie, add Refresh/LogOut endpoints`

---

## Task 17: Update Program.cs — Remove AddCookie, Add DI Registrations

**Estimated time**: 3 min

Remove the `AddCookie()` authentication registration, keep only `AddJwtBearer`, and add `IRefreshTokenRepository` DI registration.

```csharp
// SmartWallet/Program.cs — full file after change
using Nasurino.SmartWallet.Services.Validators;
using Nasurino.SmartWallet.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Services;
using Nasurino.SmartWallet.Services.AutoMappers;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Common.Infrastructure;
using Nasurino.SmartWallet.AutoMappers;
using Nasurino.SmartWallet.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Service.Infrastructure;
using Services.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Service.Infrastructure.Contracts;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Services.Contracts;
using Hangfire;
using Hangfire.PostgreSql;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;
using Nasurino.SmartWallet.Services.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

if (args.Length > 0)
{
    var migrationIndex = Array.IndexOf(args, "-m");
    if (migrationIndex != -1)
    {
        var connectionIndex = migrationIndex + 1;
        if (connectionIndex < args.Length && !string.IsNullOrEmpty(args[connectionIndex]))
        {
            var options = new DbContextOptionsBuilder<SmartWalletContext>()
                .UseNpgsql(args[connectionIndex])
                .Options;
            await SmartWalletMigrator.MigrateAsync(options);
            return;
        }
        else
        {
            throw new ArgumentException("Ожидалась строка подключения после ключа -m, но ничего не найдено");
        }
    }
}

// Add services to the container.

builder.Services.AddDbContext<SmartWalletContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("SmartWalletConnectionString")));

builder.Services.AddHangfire(conf => conf
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection"))));
builder.Services.AddHangfireServer();

builder.Services.AddControllers(x =>
{
    x.Filters.Add(typeof(SmartWalletExceptionFilter));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
			Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = 
            "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
	        "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
	        "Example: \"Bearer 12345abcdef\""
        });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
			new OpenApiSecurityScheme()
		    {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
				},
                Scheme = "oauth2",
                Name =  JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
		}
    });
});

var allowedOrigins = builder.Configuration.GetSection("ApiSettings:CqrsSettings:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins);
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
    });
});

#region Регистрация классов конфигурации
builder.Services.Configure<JwtOptions>(builder.Configuration
    .GetSection("ApiSettings:JwtSettings"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtOptions>>().Value);
builder.Services.Configure<BCryptOptions>(builder.Configuration
    .GetSection("ApiSettings:BCryptSettings"));
#endregion

#region Регистрация сервисов
builder.Services.AddAutoMapper(typeof(ServiceModelMapper));
builder.Services.AddAutoMapper(typeof(ApiModelMapper));
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
	var jwtOptions = new JwtOptions();
	builder.Configuration.GetSection("ApiSettings:JwtSettings").Bind(jwtOptions);
    x.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddScoped<IIdentityProvider, ApiIdentityProvider>();
builder.Services.AddScoped<IFinancialCalculator, FinancialCalculator>();

builder.Services.AddScoped<IDataStorageContext, SmartWalletContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionEndpointRepository, TransactionEndpointRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<ITransactionEndpointService, TransactionEndpointService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<ISmartWalletValidateService, SmartWalletValidateService>();
builder.Services.AddScoped<IClearCategoryCacheService, ClearCategoryCacheService>();
#endregion

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#region Регистрация cron задач
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider
        .GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IClearCategoryCacheService>(
        "clear-category-cache",
        service => service.ClearCategoryCacheAsync(),
        Cron.Monthly);
}
#endregion

app.Run();
```

Key changes from original:
1. **Removed** the `AddCookie()` authentication block (lines 104-114 in original)
2. **Added** `IRefreshTokenRepository` / `RefreshTokenRepository` DI registration
3. **Added** `builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtOptions>>().Value)` so that `JwtOptions` can be injected directly (needed by `UserService` and `UserController`)
4. **Added** `using Microsoft.Extensions.Options;` for `IOptions<JwtOptions>`
5. **Kept** `AllowCredentials()` in CORS (already present)
6. **Kept** middleware order: `UseCors()` → `UseAuthentication()` → `UseAuthorization()`

**Commit**: `feat: update Program.cs — remove AddCookie, add RefreshTokenRepository DI`

---

## Task 18: Update MockedUnitOfWork — Add RefreshTokenRepository

**Estimated time**: 2 min

```csharp
// UnitTests.Services.Infrastructure/MockedUnitOfWork.cs — full file after change
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;

namespace Nasurino.SmartWallet.UnitTests.Services.Infrastructure;

/// <summary>
/// Замоканый unit of work
/// </summary>
public sealed class MockedUnitOfWork : IUnitOfWork
{
    private readonly Mock<ITransactionEndpointRepository> _mockedCashVaultRepository;
    private readonly Mock<ITransactionRepository> _mockedTransactionRepository;
    private readonly Mock<IUserRepository> _mockedUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockedRefreshTokenRepository;

    ITransactionEndpointRepository IUnitOfWork.TransactionEndpointRepository => _mockedCashVaultRepository.Object;

    ITransactionRepository IUnitOfWork.TransactionRepository => _mockedTransactionRepository.Object;

    IUserRepository IUnitOfWork.UserRepository => _mockedUserRepository.Object;

    IRefreshTokenRepository IUnitOfWork.RefreshTokenRepository => _mockedRefreshTokenRepository.Object;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="MockedUnitOfWork"/>
    /// </summary>
    public MockedUnitOfWork(Mock<ITransactionEndpointRepository>? mockedCashVaultRepository = null,
        Mock<ITransactionRepository>? mockedTransactionRepository = null,
        Mock<IUserRepository>? mockedUserRepository = null,
        Mock<IRefreshTokenRepository>? mockedRefreshTokenRepository = null)
    {
        _mockedCashVaultRepository = mockedCashVaultRepository ?? new Mock<ITransactionEndpointRepository>();
        _mockedTransactionRepository =  mockedTransactionRepository ?? new Mock<ITransactionRepository>();
        _mockedUserRepository = mockedUserRepository ?? new Mock<IUserRepository>();
        _mockedRefreshTokenRepository = mockedRefreshTokenRepository ?? new Mock<IRefreshTokenRepository>();
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

**Commit**: `feat: add RefreshTokenRepository mock to MockedUnitOfWork`

---

## Task 19: Update UserServiceTests — Fix LogInAsync and Add Refresh/Logout Tests

**Estimated time**: 5 min

Update the existing test class to accommodate the new `LogInAsync` return type and add tests for `RefreshAsync` and `LogoutAsync`.

```csharp
// Service.Tests/UserServiceTests.cs — full file after change
using AutoMapper;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Services.AutoMappers;
using Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;
using Service.Infrastructure.Contracts;
using Services.Contracts;
using Services.Contracts.Models.Exceptions;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

/// <summary>
/// Тесты на UserService
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISmartWalletValidateService> _validateServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly JwtOptions _jwtOptions;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _mapperMock = new Mock<IMapper>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _jwtOptions = new JwtOptions { Key = "test-key-for-unit-tests-min-16-chars", ExpiresMinutes = 15, RefreshExpiresDays = 7 };

        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokenRepository).Returns(_refreshTokenRepositoryMock.Object);

        _userService = new UserService(
            _unitOfWorkMock.Object,
            _validateServiceMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object,
            _mapperMock.Object,
            _jwtOptions);
    }

    /// <summary>
    /// ChangePasswordAsync Should Change Password When Old Password Is Correct
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldChangePassword_WhenOldPasswordIsCorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _passwordHasherMock.Setup(x => x.Generate(It.IsAny<string>()))
            .Returns("hashedPassword");

        // Act
        await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ChangePasswordAsync Should Throw AuthenticationServiceException When Old Password Is Wrong
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowAuthenticationServiceException_WhenOldPasswordIsWrong()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "wrongOldPassword",
            NewPassword = "newPassword"
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var action = async () => await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// ChangePasswordAsync Should Throw EntityNotFoundByIdServiceException When User Not Found
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowEntityNotFoundByIdServiceException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var action = async () => await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundByIdServiceException<User>>();
    }

    /// <summary>
    /// ChangePasswordAsync Should Throw ValidationException When Validation Fails
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        var expectedValidationError = new PropertyValidationError(
            nameof(ChangePasswordModel),
            "Validation failed.");

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Throws(new SmartWalletValidationException(expectedValidationError));

        // Act
        var action = async () => await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SmartWalletValidationException>()
            .WithMessage("*Validation failed*");
    }

    /// <summary>
    /// LogInAsync Should Return AccessToken And RefreshToken When Credentials Are Valid
    /// </summary>
    [Fact]
    public async Task LogInAsync_ShouldReturnAccessTokenAndRefreshToken_WhenCredentialsAreValid()
    {
        // Arrange
        var model = new LogInModel { Email = "test@test.com", Password = "password123" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", HashedPassword = "hashed" };

        _validateServiceMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(model.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(model.Password, user.HashedPassword))
            .Returns(true);
        _mapperMock.Setup(m => m.Map<UserModel>(user)).Returns(new UserModel { Id = user.Id });
        _jwtProviderMock.Setup(j => j.GenerateToken(It.IsAny<UserModel>()))
            .Returns("access-token");

        // Act
        var result = await _userService.LogInAsync(model, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// LogInAsync Should Throw AuthenticationServiceException When Password Is Wrong
    /// </summary>
    [Fact]
    public async Task LogInAsync_ShouldThrowAuthenticationServiceException_WhenPasswordIsWrong()
    {
        // Arrange
        var model = new LogInModel { Email = "test@test.com", Password = "wrong" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", HashedPassword = "hashed" };

        _validateServiceMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(model.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(model.Password, user.HashedPassword))
            .Returns(false);

        // Act
        var action = async () => await _userService.LogInAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// RefreshAsync Should Return New Tokens When Refresh Token Is Valid
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-refresh-token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        var user = new User { Id = userId };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserModel>(user)).Returns(new UserModel { Id = userId });
        _jwtProviderMock.Setup(j => j.GenerateToken(It.IsAny<UserModel>()))
            .Returns("new-access-token");

        // Act
        var result = await _userService.RefreshAsync("valid-refresh-token", CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _refreshTokenRepositoryMock.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// RefreshAsync Should Throw AuthenticationServiceException When Refresh Token Not Found
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldThrowAuthenticationServiceException_WhenRefreshTokenNotFound()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("unknown-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var action = async () => await _userService.RefreshAsync("unknown-token", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// RefreshAsync Should Throw AuthenticationServiceException When Refresh Token Is Expired
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldThrowAuthenticationServiceException_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var action = async () => await _userService.RefreshAsync("expired-token", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// RefreshAsync Should Throw AuthenticationServiceException When Refresh Token Is Revoked
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldThrowAuthenticationServiceException_WhenRefreshTokenIsRevoked()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "revoked-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = DateTime.UtcNow.AddDays(-1)
        };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("revoked-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var action = async () => await _userService.RefreshAsync("revoked-token", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// LogoutAsync Should Revoke Refresh Token When Token Exists
    /// </summary>
    [Fact]
    public async Task LogoutAsync_ShouldRevokeRefreshToken_WhenTokenExists()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        await _userService.LogoutAsync("valid-token", CancellationToken.None);

        // Assert
        storedToken.RevokedAt.Should().NotBeNull();
        _refreshTokenRepositoryMock.Verify(r => r.Update(storedToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// LogoutAsync Should Do Nothing When Token Does Not Exist
    /// </summary>
    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenDoesNotExist()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("unknown-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        await _userService.LogoutAsync("unknown-token", CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

**Commit**: `feat: update UserServiceTests — fix LogInAsync, add Refresh/Logout tests`

---

## Task 20: Generate and Apply EF Core Migration

**Estimated time**: 3 min

Run the migration commands to create the `RefreshToken` table in the database.

```bash
dotnet ef migrations add AddRefreshToken -p DAL/Context -s SmartWallet
dotnet ef database update -p DAL/Context -s SmartWallet
```

**Commit**: `feat: add EF Core migration for RefreshToken entity`

---

## Task 21: Build and Verify Compilation

**Estimated time**: 2 min

Run a full build to verify everything compiles:

```bash
dotnet build SmartWallet.sln
```

Run all tests:

```bash
dotnet test SmartWallet.sln
```

**Commit**: (no commit needed — verification step)

---

## Summary of Commits (in order)

| # | Commit Message |
|---|----------------|
| 1 | `feat: add RefreshToken entity` |
| 2 | `feat: add RefreshTokens navigation property to User entity` |
| 3 | `feat: add RefreshToken EF Core configuration` |
| 4 | `feat: add RefreshTokens DbSet to SmartWalletContext` |
| 5 | `feat: add IRefreshTokenRepository interface` |
| 6 | `feat: add RefreshTokenRepository implementation` |
| 7 | `feat: add RefreshTokenRepository to IUnitOfWork` |
| 8 | `feat: add RefreshTokenRepository to UnitOfWork` |
| 9 | `feat: update JwtOptions — ExpiresMinutes + RefreshExpiresDays` |
| 10 | `feat: update JwtProvider to use ExpiresMinutes` |
| 11 | `feat: update appsettings.json — ExpiresMinutes + RefreshExpiresDays` |
| 12 | `feat: update IUserService — LogInAsync returns tuple, add RefreshAsync/LogoutAsync` |
| 13 | `feat: implement LogInAsync tuple return, RefreshAsync, LogoutAsync in UserService` |
| 14 | `feat: update ResponseLogInApiModel — JwtToken → AccessToken` |
| 15 | `feat: add ResponseRefreshApiModel` |
| 16 | `feat: rewrite UserController — LogIn with cookie, add Refresh/LogOut endpoints` |
| 17 | `feat: update Program.cs — remove AddCookie, add RefreshTokenRepository DI` |
| 18 | `feat: add RefreshTokenRepository mock to MockedUnitOfWork` |
| 19 | `feat: update UserServiceTests — fix LogInAsync, add Refresh/Logout tests` |
| 20 | `feat: add EF Core migration for RefreshToken entity` |

## Dependency Graph

```
Task 1 (RefreshToken entity)
  └── Task 2 (User.RefreshTokens nav prop)
  └── Task 3 (RefreshTokenConfiguration)
  └── Task 4 (DbSet<RefreshToken>)
  └── Task 5 (IRefreshTokenRepository)
       └── Task 6 (RefreshTokenRepository)
            └── Task 7 (IUnitOfWork update)
                 └── Task 8 (UnitOfWork update)
Task 9 (JwtOptions)
  └── Task 10 (JwtProvider)
  └── Task 11 (appsettings.json)
Task 5 + Task 12 (IUserService)
  └── Task 13 (UserService)
Task 14 (ResponseLogInApiModel)
Task 15 (ResponseRefreshApiModel)
Task 13 + Task 14 + Task 15 + Task 9
  └── Task 16 (UserController)
Task 8 + Task 6
  └── Task 17 (Program.cs)
Task 7
  └── Task 18 (MockedUnitOfWork)
Task 13 + Task 18
  └── Task 19 (UserServiceTests)
Task 1-8 + Task 20 (EF migration)
All tasks → Task 21 (Build verification)
```

## Risk Assessment

| Risk | Mitigation |
|------|-----------|
| Existing `LogInAsync` callers break | Only `UserController.LogIn` calls it; updated in Task 16 |
| `ExpiresHours` removal breaks config binding | `appsettings.json` updated in Task 11; `JwtOptions` updated in Task 9 |
| `AddCookie()` removal breaks auth | `AddJwtBearer` remains; cookie is set manually via `Response.Cookies` |
| EF migration fails | Migration generated after all entity/config changes (Task 20) |
| Test compilation errors | `UserService` constructor signature changed; `MockedUnitOfWork` updated in Task 18 |
| `AuthenticationServiceException` from `RefreshAsync` not mapped to 401 | Already mapped in `SmartWalletExceptionFilter` (line 25-33) |