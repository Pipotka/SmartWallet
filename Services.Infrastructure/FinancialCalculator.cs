using Service.Infrastructure.Contracts;

namespace Nasurino.SmartWallet.Service.Infrastructure;

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
}