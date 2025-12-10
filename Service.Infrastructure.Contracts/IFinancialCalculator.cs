namespace Service.Infrastructure.Contracts;

/// <summary>
/// Финансовый калькулятор
/// </summary>
public interface IFinancialCalculator
{
    /// <summary>
    /// Вычисляет процентное соотношение части от суммы.
    /// Возвращает 0, если сумма меньше или равна нулю.
    /// </summary>
    /// <param name="sum">Общая сумма (знаменатель)</param>
    /// <param name="part">Часть от суммы (числитель)</param>
    /// <param name="decimals">Количество знаков после запятой в результате (по умолчанию 2)</param>
    /// <returns>Процентное соотношение, округленное до указанного количества знаков после запятой</returns>
    double GetPercentage(double sum, double part, int decimals = 2);

    /// <summary>
    /// Вычисляет значение, соответствующее указанному проценту от суммы.
    /// </summary>
    /// <param name="sum">Исходная сумма</param>
    /// <param name="percentage">Процент (может быть больше 100)</param>
    /// <returns>
    /// Значение, соответствующее указанному проценту от суммы.
    /// Возвращает 0, если процент меньше 0 или сумма меньше 0.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = CalculatePercentageOfSum(1000, 15); // Возвращает 150
    /// var result2 = CalculatePercentageOfSum(500, 50); // Возвращает 250
    /// var result3 = CalculatePercentageOfSum(1000, 150); // Возвращает 1500
    /// var result4 = CalculatePercentageOfSum(1000, -10); // Возвращает 0
    /// </code>
    /// </example>
    double PercentageOfSum(double sum, double percentage);
}