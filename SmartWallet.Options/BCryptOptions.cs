namespace Nasurino.SmartWallet.Options;

/// <summary>
/// Найстройки конфигурацииы для BCrypt
/// </summary>
public class BCryptOptions
{
    /// <summary>
    /// Количество раундов хэширования
    /// </summary>
    public int WorkFactor { get; set; }
}
