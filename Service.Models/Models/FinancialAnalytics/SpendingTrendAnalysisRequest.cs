namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Запрос на анализ трендов трат
/// </summary>
public sealed class SpendingTrendAnalysisRequest
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }
	
	/// <summary>
	///	Дата окончания первого (прошлого) периода
	/// </summary>
	public DateOnly FirstDate { get; set; }
	
	/// <summary>
	/// Дата окончания второго (текущего) периода 
	/// </summary>
	public DateOnly SecondDate { get; set; }
	
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
		var startDate = GetStartDate(FirstDate);
		var endDate = GetEndDate(FirstDate);
		return (startDate, endDate);
	}
	
	/// <summary>
	/// Возвращает второй период
	/// </summary>
	public (DateOnly Start, DateOnly End) GetSecondDateRange()
	{
		var startDate = GetStartDate(SecondDate);
		var endDate = GetEndDate(SecondDate);
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