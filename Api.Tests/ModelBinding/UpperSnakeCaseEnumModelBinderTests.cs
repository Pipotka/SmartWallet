using System.Globalization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nasurino.SmartWallet.Entities.Enums;
using Nasurino.SmartWallet.Infrastructure.ModelBinding;
using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;
using Xunit;

namespace Nasurino.SmartWallet.Api.Tests.ModelBinding;

public class UpperSnakeCaseEnumModelBinderTests
{
    private readonly UpperSnakeCaseEnumModelBinder _binder = new();

    [Theory]
    [InlineData("TRANSFER", TransactionType.Transfer)]
    [InlineData("ADJUSTMENT_INCREASE", TransactionType.AdjustmentIncrease)]
    [InlineData("FOR_TEST", TransactionType.ForTest)]
    [InlineData("CATEGORY", EndpointType.Category)]
    [InlineData("STORAGE", EndpointType.Storage)]
    [InlineData("DAY", TimeUnit.Day)]
    [InlineData("MONTH", TimeUnit.Month)]
    [InlineData("YEAR", TimeUnit.Year)]
    public async Task BindModelAsync_ShouldParseUpperSnakeCaseEnum(string value, object expected)
    {
        var context = CreateBindingContext(value, expected.GetType());

        await _binder.BindModelAsync(context);

        context.Result.IsModelSet.Should().BeTrue();
        context.Result.Model.Should().Be(expected);
    }

    [Theory]
    [InlineData("TRANSFER", TransactionType.Transfer)]
    [InlineData("ADJUSTMENT_INCREASE", TransactionType.AdjustmentIncrease)]
    public async Task BindModelAsync_ForNullableEnum_ShouldParseValue(string value, TransactionType expected)
    {
        var context = CreateBindingContext(value, typeof(TransactionType?));

        await _binder.BindModelAsync(context);

        context.Result.IsModelSet.Should().BeTrue();
        context.Result.Model.Should().Be(expected);
    }

    [Fact]
    public async Task BindModelAsync_ForNullableEnum_WithEmptyValue_ShouldLeaveModelUnset()
    {
        var context = CreateBindingContext(string.Empty, typeof(TransactionType?));

        await _binder.BindModelAsync(context);

        context.Result.IsModelSet.Should().BeFalse();
    }

    [Fact]
    public async Task BindModelAsync_ForInvalidValue_ShouldAddModelError()
    {
        var context = CreateBindingContext("NOT_AN_ENUM", typeof(TransactionType));

        await _binder.BindModelAsync(context);

        context.Result.IsModelSet.Should().BeFalse();
        context.ModelState.Should().ContainSingle();
        context.ModelState.First().Value!.Errors.Should().ContainSingle();
    }

    private static DefaultModelBindingContext CreateBindingContext(string value, Type modelType)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(modelType);

        return new DefaultModelBindingContext
        {
            ModelName = "type",
            ModelState = new ModelStateDictionary(),
            ModelMetadata = metadata,
            ValueProvider = new SimpleValueProvider(new Dictionary<string, string> { { "type", value } })
        };
    }

    private class SimpleValueProvider(Dictionary<string, string> values) : IValueProvider
    {
        public bool ContainsPrefix(string prefix) => values.Keys.Any(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        public ValueProviderResult GetValue(string key)
            => values.TryGetValue(key, out var v) ? new ValueProviderResult(v) : ValueProviderResult.None;
    }
}
