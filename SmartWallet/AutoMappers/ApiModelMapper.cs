using AutoMapper;
using Nasurino.SmartWallet.Models.Account;
using Nasurino.SmartWallet.Models.CashVault;
using Nasurino.SmartWallet.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Models.Transaction;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Nasurino.SmartWallet.AutoMappers;

/// <summary>
/// Маппер моделей сервиса
/// </summary>
public class ApiModelMapper : Profile
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="ApiModelMapper"/>
	/// </summary>
	public ApiModelMapper()
	{
		CreateMap<UserModel, UserApiModel>(MemberList.Destination);
		CreateMap<CreateUserApiModel, CreateUserModel>(MemberList.Destination);
		CreateMap<RequestLogInApiModel, LogInModel>(MemberList.Destination);
		CreateMap<UpdateUserApiModel, UpdateUserModel>(MemberList.Destination)
			.ForMember(x => x.Id, opt => opt.Ignore());
		CreateMap<DeleteUserApiModel, DeleteUserModel>(MemberList.Destination)
			.ForMember(x => x.Id, opt => opt.Ignore());

		CreateMap<TransactionModel, TransactionApiModel>(MemberList.Destination);
		CreateMap<CreateTransactionApiModel, CreateTransactionModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());
		CreateMap<DeleteTransactionApiModel, DeleteTransactionModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());

		CreateMap<TransactionEndpointModel, TransactionEndpointApiModel>(MemberList.Destination);
		CreateMap<CreateTransactionEndpointApiModel, CreateTransactionEndpointModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());
		CreateMap<UpdateTransactionEndpointApiModel, UpdateTransactionEndpointModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());
		CreateMap<DeleteTransactionEndpointApiModel, DeleteTransactionEndpointModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());

		CreateMap<CategorySpendingItemModel, CategorySpendingItemApiModel>(MemberList.Destination);
		CreateMap<SpendingCategoryModel, CategorizingSpendingApiResponse>(MemberList.Destination);
	}
}
