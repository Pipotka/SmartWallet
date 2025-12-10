using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nasurino.SmartWallet.Context.Contracts;

namespace Nasurino.SmartWallet.Context.Tests;

/// <summary>
/// Класс <see cref="SmartWalletContext"/> для тестов с базой в памяти. Один контекст на тест
/// </summary>
public class SmartWalletContextInMemory : IAsyncDisposable
{
    /// <summary>
    /// Контекст <see cref="SmartWalletContext"/>
    /// </summary>
    protected SmartWalletContext Context { get; }
    
    /// <summary>
    /// Хранилище данных
    /// </summary>
    protected IDataStorageContext StorageContext => Context;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="SmartWalletContextInMemory"/>
    /// </summary>
    protected SmartWalletContextInMemory()
    {
        var optionsBuilder = new DbContextOptionsBuilder<SmartWalletContext>()
            .UseInMemoryDatabase($"SmartWalletTests{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        Context = new SmartWalletContext(optionsBuilder.Options);
    }
    
    public async ValueTask DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }
}