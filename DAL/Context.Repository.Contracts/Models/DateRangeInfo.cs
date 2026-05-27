namespace Nasurino.SmartWallet.Context.Repository.Contracts.Models;

/// <summary>
/// Информация о временном диапазоне для агрегации данных
/// </summary>
public sealed class DateRangeInfo
{
    /// <summary>
    /// Начало временного диапазона (включительно)
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Конец временного диапазона (исключительно)
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Метка временного диапазона
    /// </summary>
    public string Label { get; set; } = string.Empty;
}