namespace Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics
{
    /// <summary>
    /// Запрос на получение категоризированных трат
    /// </summary>
    public class CategorizingSpendingRequest
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Начало временного диапазона
        /// </summary>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// Конец временного диапазона
        /// </summary>
        /// <remarks>Исключенный верхний предел временного диапазона</remarks>
        public DateOnly EndDate { get; set; }
    }
}
