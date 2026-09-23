using System.Text.Json;

namespace Nasurino.SmartWallet.Infrastructure.Json;

public sealed class UpperSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
        => JsonNamingPolicy.SnakeCaseLower.ConvertName(name).ToUpperInvariant();
}
