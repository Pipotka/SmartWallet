using System.Linq.Expressions;

namespace Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;

/// <summary>
/// Провайдер для постановки фоновых задач в очередь.
/// Абстрагирует конкретную реализацию (Hangfire и т.д.) от слоя сервисов.
/// </summary>
public interface IBackgroundTaskSystemProvider
{
	/// <summary>
	/// Ставит вызов метода в очередь с выполнением "огонь-и-забыл" (fire-and-forget).
	/// Задача выполнится асинхронно в фоне, без ожидания вызывающим кодом.
	/// </summary>
	/// <typeparam name="T">Тип сервиса, метод которого вызывается</typeparam>
	/// <param name="method">Выражение вызова метода сервиса (например, x => x.ProcessAsync(id))</param>
	void FireAndForget<T>(Expression<Func<T, Task>> method);
}
