using System.Globalization;

namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Запрос на получение данных линейного графика трат по категориям
/// </summary>
public sealed class SpendingTrendLineRequest
{
    private static readonly string[] RussianMonthNamesNominative =
        ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"];

    private static readonly string[] RussianMonthNamesGenitive =
        ["января", "февраля", "марта", "апреля", "мая", "июня",
        "июля", "августа", "сентября", "октября", "ноября", "декабря"];

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Дата начала диапазона
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Дата окончания диапазона
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Единица измерения временного периода
    /// </summary>
    public TimeUnit TimeUnit { get; set; }

    /// <summary>
    /// Возвращает список временных диапазонов с метками для линейного графика
    /// </summary>
    public IReadOnlyList<(DateOnly Start, DateOnly End, string Label)> GetDateRanges()
    {
        var spansMultipleMonths = TimeUnit == TimeUnit.Day &&
            (StartDate.Year != EndDate.Year || StartDate.Month != EndDate.Month);
        var spansMultipleYears = TimeUnit == TimeUnit.Month &&
            StartDate.Year != EndDate.Year;

        var ranges = new List<(DateOnly Start, DateOnly End, string Label)>();

        switch (TimeUnit)
        {
            case TimeUnit.Day:
                var currentDay = StartDate;
                while (currentDay <= EndDate)
                {
                    var label = spansMultipleMonths
                        ? $"{currentDay.Day} {RussianMonthNamesGenitive[currentDay.Month - 1]}"
                        : currentDay.Day.ToString();
                    ranges.Add((currentDay, currentDay, label));
                    currentDay = currentDay.AddDays(1);
                }
                break;

            case TimeUnit.Month:
                var currentMonth = new DateOnly(StartDate.Year, StartDate.Month, 1);
                while (currentMonth <= EndDate)
                {
                    var periodEnd = new DateOnly(currentMonth.Year,
                        currentMonth.Month,
                        DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                    var label = spansMultipleYears
                        ? $"{RussianMonthNamesNominative[currentMonth.Month - 1]} {currentMonth.Year}"
                        : RussianMonthNamesNominative[currentMonth.Month - 1];
                    ranges.Add((currentMonth, periodEnd, label));
                    currentMonth = currentMonth.AddMonths(1);
                }
                break;

            case TimeUnit.Year:
                var currentYear = new DateOnly(StartDate.Year, 1, 1);
                while (currentYear <= EndDate)
                {
                    var periodEnd = new DateOnly(currentYear.Year, 12, 31);
                    var label = currentYear.Year.ToString();
                    ranges.Add((currentYear, periodEnd, label));
                    currentYear = currentYear.AddYears(1);
                }
                break;
        }

        return ranges;
    }
}