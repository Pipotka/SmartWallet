using System.Linq.Expressions;
using Hangfire;
using Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;

namespace Nasurino.SmartWallet.BackgroundTaskSystem.Hangfire;

/// <summary>
/// Реализация <see cref="IBackgroundTaskSystemProvider"/> поверх Hangfire.
/// Использует BackgroundJob.Enqueue для post-and-forget выполнения.
/// </summary>
public sealed class HangfireBackgroundTaskSystemProvider : IBackgroundTaskSystemProvider
{
	/// <inheritdoc/>
	public void FireAndForget<T>(Expression<Func<T, Task>> method)
	{
		BackgroundJob.Enqueue(method);
	}
}
