using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Nasurino.SmartWallet.Infrastructure.ModelBinding;

public class UpperSnakeCaseEnumModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var enumType = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;
        if (!enumType.IsEnum)
        {
            return Task.CompletedTask;
        }

        try
        {
            var pascalCaseName = ConvertToPascalCase(value);
            var result = Enum.Parse(enumType, pascalCaseName, ignoreCase: true);
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.TryAddModelError(modelName, ex, bindingContext.ModelMetadata);
        }

        return Task.CompletedTask;
    }

    private static string ConvertToPascalCase(string upperSnakeCase)
    {
        var parts = upperSnakeCase.Split('_');
        var textInfo = CultureInfo.InvariantCulture.TextInfo;

        return string.Concat(parts.Select(part =>
        {
            if (string.IsNullOrEmpty(part))
            {
                return string.Empty;
            }

            var lower = part.ToLowerInvariant();
            return textInfo.ToTitleCase(lower);
        }));
    }
}
