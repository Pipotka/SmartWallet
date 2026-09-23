using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Nasurino.SmartWallet.Entities.Enums;
using Nasurino.SmartWallet.Infrastructure.Json;
using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;
using Xunit;

namespace Nasurino.SmartWallet.Api.Tests.Json;

public class ApiEnumSerializationTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter(new UpperSnakeCaseNamingPolicy()) }
    };

    [Theory]
    [InlineData(TransactionType.Transfer, "TRANSFER")]
    [InlineData(TransactionType.AdjustmentIncrease, "ADJUSTMENT_INCREASE")]
    public void TransactionType_ShouldSerializeToUpperSnakeCase(TransactionType value, string expected)
    {
        JsonSerializer.Serialize(value, _options).Should().Be($"\"{expected}\"");
    }

    [Theory]
    [InlineData("TRANSFER", TransactionType.Transfer)]
    [InlineData("ADJUSTMENT_INCREASE", TransactionType.AdjustmentIncrease)]
    public void TransactionType_ShouldDeserializeFromUpperSnakeCase(string value, TransactionType expected)
    {
        JsonSerializer.Deserialize<TransactionType>($"\"{value}\"", _options).Should().Be(expected);
    }

    [Theory]
    [InlineData(EndpointType.Category, "CATEGORY")]
    [InlineData(EndpointType.Storage, "STORAGE")]
    public void EndpointType_ShouldSerializeToUpperSnakeCase(EndpointType value, string expected)
    {
        JsonSerializer.Serialize(value, _options).Should().Be($"\"{expected}\"");
    }

    [Theory]
    [InlineData("CATEGORY", EndpointType.Category)]
    [InlineData("STORAGE", EndpointType.Storage)]
    public void EndpointType_ShouldDeserializeFromUpperSnakeCase(string value, EndpointType expected)
    {
        JsonSerializer.Deserialize<EndpointType>($"\"{value}\"", _options).Should().Be(expected);
    }

    [Theory]
    [InlineData(TimeUnit.Day, "DAY")]
    [InlineData(TimeUnit.Month, "MONTH")]
    [InlineData(TimeUnit.Year, "YEAR")]
    public void TimeUnit_ShouldSerializeToUpperSnakeCase(TimeUnit value, string expected)
    {
        JsonSerializer.Serialize(value, _options).Should().Be($"\"{expected}\"");
    }

    [Theory]
    [InlineData("DAY", TimeUnit.Day)]
    [InlineData("MONTH", TimeUnit.Month)]
    [InlineData("YEAR", TimeUnit.Year)]
    public void TimeUnit_ShouldDeserializeFromUpperSnakeCase(string value, TimeUnit expected)
    {
        JsonSerializer.Deserialize<TimeUnit>($"\"{value}\"", _options).Should().Be(expected);
    }
}
