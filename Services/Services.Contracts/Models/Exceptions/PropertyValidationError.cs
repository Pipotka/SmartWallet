namespace Nasurino.SmartWallet.Services.Contracts.Models.Exceptions;

/// <summary>
/// Ошибка валидации свойства модели
/// </summary>
/// <param name="PropertyName">Название свойства</param>
/// <param name="ErrorMessage">Сообщение об ошибке</param>
public sealed record PropertyValidationError(string PropertyName, string ErrorMessage);