using Nasurino.SmartWallet.Services.Infrastructure.Contracts;

namespace Nasurino.SmartWallet.Services.Infrastructure;

/// <inheritdoc cref="IFinancialCalculator"/>
public class FinancialCalculator : IFinancialCalculator
{
    public double GetPercentage(double sum, double part, int decimals = 2)
        => sum <= 0.0 ? 0.0 : Math.Round((part / sum) * 100, decimals);

    public double PercentageOfSum(double sum, double percentage)
    {
        if (percentage < 0 || sum < 0)
            return 0.0;
    
        return sum * (percentage / 100);
    }

    public double CalculateTrendPercentage(double currentValue, double previousValue, int decimals = 2)
    {
        if (currentValue < 0)
        {
            throw new ArgumentException("Текущее значение не может быть отрицательным", nameof(currentValue));
        }

        if (previousValue < 0)
        {
            throw new ArgumentException("Предыдущее значение не может быть отрицательным", nameof(previousValue));
        }

        if (previousValue == 0)
        {
            return 0.0;
        }
        
        var trend = ((currentValue - previousValue) / previousValue) * 100;
        return Math.Round(trend, decimals);
    }
}