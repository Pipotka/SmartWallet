using AutoMapper;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Nasurino.SmartWallet.Service.Models.Mappings;

/// <summary>
/// Мапер моделей сервиса
/// </summary>
public class ServiceModelMapper : Profile
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="ServiceModelMapper"/>
	/// </summary>
	public ServiceModelMapper()
	{
		CreateMap<User, UserModel>(MemberList.Destination);
		CreateMap<CreateUserModel, User>(MemberList.Destination)
			.ForMember(dest => dest.Transactions, opt => opt.Ignore())
			.ForMember(dest => dest.CashVaults, opt => opt.Ignore())
			.ForMember(dest => dest.SpendingAreas, opt => opt.Ignore())
			.ForMember(dest => dest.HashedPassword, opt => opt.Ignore());
		CreateMap<UpdateUserModel, User>(MemberList.Destination);
	}
}
