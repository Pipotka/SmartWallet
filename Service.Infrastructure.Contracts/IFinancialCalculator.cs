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

    /// <summary>
    /// Вычисляет процент изменения (динамику) между текущим и предыдущим значением.
    /// </summary>
    /// <param name="currentValue">Текущее значение</param>
    /// <param name="previousValue">Предыдущее значение</param>
    /// <param name="decimals">Количество знаков после запятой в результате (по умолчанию 2)</param>
    /// <returns>
    /// Процент изменения, округленный до указанного количества знаков после запятой.
    /// Возвращает 0, если previousValue равно 0.
    /// </returns>
    /// <exception cref="ArgumentException">Выбрасывается, если currentValue или previousValue отрицательны</exception>
    /// <example>
    /// <code>
    /// var result = CalculateTrendPercentage(150, 100); // Возвращает 50.00 (рост на 50%)
    /// var result2 = CalculateTrendPercentage(50, 100); // Возвращает -50.00 (снижение на 50%)
    /// var result3 = CalculateTrendPercentage(100, 100); // Возвращает 0.00 (без изменений)
    /// var result4 = CalculateTrendPercentage(200, 0); // Возвращает 0 (база равна нулю)
    /// </code>
    /// </example>
    double CalculateTrendPercentage(double currentValue, double previousValue, int decimals = 2);
}