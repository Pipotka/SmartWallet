using FluentAssertions;
using Nasurino.SmartWallet.Infrastructure.Json;
using Xunit;

namespace Nasurino.SmartWallet.Api.Tests.Json;

public class UpperSnakeCaseNamingPolicyTests
{
    [Theory]
    [InlineData("Transfer", "TRANSFER")]
    [InlineData("AdjustmentIncrease", "ADJUSTMENT_INCREASE")]
    [InlineData("EndpointType", "ENDPOINT_TYPE")]
    public void ConvertName_ShouldReturnUpperSnakeCase(string input, string expected)
    {
        new UpperSnakeCaseNamingPolicy().ConvertName(input).Should().Be(expected);
    }
}
