using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Nasurino.SmartWallet.Infrastructure.ModelBinding;

public class UpperSnakeCaseEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.UnderlyingOrModelType;
        return modelType.IsEnum ? new UpperSnakeCaseEnumModelBinder() : null;
    }
}
