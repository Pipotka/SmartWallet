namespace Nasurino.SmartWallet.Service.Infrastructure
{
	/// <summary>
	/// Интерфейс хэшера паролей
	/// </summary>
	public interface IPasswordHasher
	{
		/// <summary>
		/// Генерирует хэш для пароля
		/// </summary>
		string Generate(string password);

		/// <summary>
		/// Проверяет эквивалентность пароля и хэша пароля
		/// </summary>
		bool Verify(string password, string hashedPassword);
	}
}