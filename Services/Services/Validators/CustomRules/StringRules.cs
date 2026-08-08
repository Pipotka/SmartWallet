using FluentValidation;

namespace Nasurino.SmartWallet.Services.Validators.CustomRules;

/// <summary>
/// Правила валидации для строк
/// </summary>
public static class StringRules
{
	/// <summary>
	/// Валидатор пароля
	/// </summary>
	public static IRuleBuilderOptions<T, string> MustBePassword<T>(this IRuleBuilder<T, string> ruleBuilder)
		=> ruleBuilder.Length(8, 33)
			.WithMessage("Пароль должен быть больше 8 и меньше 34 символов")
			.Matches(@"^(?=.*[@#$!^%&*()\-_+=]).+$")
			.WithMessage("Пароль должен содержать спецсимволы")
			.Matches(@"^(?=.*\p{Lu}).+$")
			.WithMessage("Пароль должен содержать заглавную букву");
}
