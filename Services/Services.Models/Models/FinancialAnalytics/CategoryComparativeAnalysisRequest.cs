namespace Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

/// <summary>
/// Запрос на сравнительный анализ  трат по категориям
/// </summary>
public sealed class CategoryComparativeAnalysisRequest
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }
	
	/// <summary>
	///	Дата окончания первого периода
	/// </summary>
	public DateOnly FirstPeriod { get; set; }
	
	/// <summary>
	/// Дата окончания второго периода 
	/// </summary>
	public DateOnly SecondPeriod { get; set; }
	
	/// <summary>
	/// Единица измерения временного периода 
	/// </summary>
	public TimeUnit TimeUnit { get; set; }
	
	/// <summary>
	/// Количество единиц в периоде 
	/// </summary>
	public int TimeUnitCount { get; set; }

	/// <summary>
	/// Возвращает первый период
	/// </summary>
	public (DateOnly Start, DateOnly End) GetFirstDateRange()
	{
		var startDate = GetStartDate(FirstPeriod);
		var endDate = GetEndDate(FirstPeriod);
		return (startDate, endDate);
	}
	
	/// <summary>
	/// Возвращает второй период
	/// </summary>
	public (DateOnly Start, DateOnly End) GetSecondDateRange()
	{
		var startDate = GetStartDate(SecondPeriod);
		var endDate = GetEndDate(SecondPeriod);
		return (startDate, endDate);
	}
	
	private DateOnly GetStartDate(DateOnly endDate)
	{
		var actualTimeUnitCount = TimeUnitCount - 1;
		return TimeUnit switch
		{
			TimeUnit.Day => endDate.AddDays(-actualTimeUnitCount),
			TimeUnit.Month => new DateOnly(endDate.AddMonths(-actualTimeUnitCount).Year,
				endDate.AddMonths(-actualTimeUnitCount).Month,
				1),
			TimeUnit.Year => new DateOnly(endDate.AddYears(-actualTimeUnitCount).Year, 1, 1),
			_ => throw new InvalidOperationException()
		};
	}

	private DateOnly GetEndDate(DateOnly endDate) 
		=> TimeUnit switch
		{
			TimeUnit.Day => endDate,
			TimeUnit.Month => endDate.AddDays(DateTime.DaysInMonth(endDate.Year, endDate.Month) - endDate.Day),
			TimeUnit.Year => new DateOnly(endDate.Year, 12, 31),
			_ => throw new InvalidOperationException()	
		};
}