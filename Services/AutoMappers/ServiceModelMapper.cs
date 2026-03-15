using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
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
		CreateMap<CreateUserModel, User>(MemberList.Source)
			.ForSourceMember(x => x.Password, opt => opt.DoNotValidate());
		CreateMap<UpdateUserModel, User>(MemberList.Source);
		CreateMap<TransactionEndpoint, TransactionEndpointModel>(MemberList.Destination);
		CreateMap<UpdateTransactionEndpointModel, TransactionEndpoint>(MemberList.Source);
		CreateMap<CreateTransactionEndpointModel, TransactionEndpoint>(MemberList.Source);

		CreateMap<Transaction, TransactionModel>(MemberList.Destination);
		CreateMap<CreateTransactionModel, Transaction>(MemberList.Destination)
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.Ignore())
			.ForMember(dest => dest.User, opt => opt.Ignore())
			.ForMember(dest => dest.SourceAccount, opt => opt.Ignore())
			.ForMember(dest => dest.DestinationAccount, opt => opt.Ignore())
			.ForMember(dest => dest.MadeAt, opt => opt.Ignore())
			.ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

		CreateMap<CategorySpendingItem, CategorySpendingItemModel>(MemberList.Destination);
	}
}
