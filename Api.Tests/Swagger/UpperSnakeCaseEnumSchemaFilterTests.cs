using FluentAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Nasurino.SmartWallet.Entities.Enums;
using Nasurino.SmartWallet.Infrastructure.Swagger;
using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Nasurino.SmartWallet.Api.Tests.Swagger;

public class UpperSnakeCaseEnumSchemaFilterTests
{
    private readonly UpperSnakeCaseEnumSchemaFilter _filter = new();

    [Theory]
    [InlineData(typeof(TransactionType), new[] { "TRANSFER", "EXPENSE", "ADJUSTMENT_DECREASE", "ADJUSTMENT_INCREASE", "INCOME" })]
    [InlineData(typeof(EndpointType), new[] { "CATEGORY", "STORAGE" })]
    [InlineData(typeof(TimeUnit), new[] { "DAY", "MONTH", "YEAR" })]
    public void Apply_ForEnum_ShouldProduceStringSchemaWithUpperSnakeCaseValues(Type enumType, string[] expected)
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(enumType, null, null);

        _filter.Apply(schema, context);

        schema.Type.Should().Be("string");
        schema.Enum.Should().HaveCount(expected.Length);
        schema.Enum.OfType<OpenApiString>().Select(x => x.Value).Should().Equal(expected);
    }

    [Fact]
    public void Apply_ForNonEnum_ShouldNotChangeSchemaType()
    {
        var schema = new OpenApiSchema { Type = "integer" };
        var context = new SchemaFilterContext(typeof(int), null, null);

        _filter.Apply(schema, context);

        schema.Type.Should().Be("integer");
        schema.Enum.Should().BeNullOrEmpty();
    }
}
