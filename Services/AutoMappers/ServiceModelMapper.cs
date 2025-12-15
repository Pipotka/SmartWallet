using AutoMapper;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Nasurino.SmartWallet.Services.AutoMappers;

/// <summary>
/// Маппер моделей сервиса
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
			.ForMember(dest => dest.HashedPassword, opt => opt.Ignore())
			.ForMember(dest => dest.Id, opt => opt.Ignore());
		CreateMap<UpdateUserModel, User>(MemberList.Destination)
			.ForMember(dest => dest.Transactions, opt => opt.Ignore())
			.ForMember(dest => dest.CashVaults, opt => opt.Ignore())
			.ForMember(dest => dest.HashedPassword, opt => opt.Ignore())
			.ForMember(dest => dest.Email, opt => opt.Ignore());

		CreateMap<TransactionEndpoint, TransactionEndpointModel>(MemberList.Destination)
			.ForMember(x => x.Value, opt => opt.Ignore());
		CreateMap<UpdateTransactionEndpointModel, TransactionEndpoint>(MemberList.Destination)
			.ForMember(dest => dest.Transactions, opt => opt.Ignore())
			.ForMember(dest => dest.User, opt => opt.Ignore())
			.ForMember(dest => dest.IsStorage, opt => opt.Ignore())
			.ForMember(dest => dest.Value, opt => opt.Ignore())
			.ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
		CreateMap<CreateTransactionEndpointModel, TransactionEndpoint>(MemberList.Destination)
			.ForMember(dest => dest.Transactions, opt => opt.Ignore())
			.ForMember(dest => dest.User, opt => opt.Ignore())
			.ForMember(dest => dest.Value, opt => opt.Ignore())
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

		CreateMap<Transaction, TransactionModel>(MemberList.Destination);
		CreateMap<CreateTransactionModel, Transaction>(MemberList.Destination)
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.User, opt => opt.Ignore())
			.ForMember(dest => dest.SourceAccount, opt => opt.Ignore())
			.ForMember(dest => dest.DestinationAccount, opt => opt.Ignore())
			.ForMember(dest => dest.MadeAt, opt => opt.Ignore())
			.ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
	}
}
