using AutoMapper;
using Nasurino.SmartWallet.Models.Account;
using Nasurino.SmartWallet.Models.CashVault;
using Nasurino.SmartWallet.Models.SpendingArea;
using Nasurino.SmartWallet.Models.Transaction;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
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
		CreateMap<CreateTransactionApiModel, CreateTransactionModel>(MemberList.Destination);
		CreateMap<DeleteTransactionApiModel, DeleteTransactionModel>(MemberList.Destination);

		CreateMap<SpendingAreaModel, SpendingAreaApiModel>(MemberList.Destination);
		CreateMap<CreateSpendingAreaApiModel, CreateSpendingAreaModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());
		CreateMap<UpdateSpendingAreaApiModel, UpdateSpendingAreaModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());
		CreateMap<DeleteSpendingAreaApiModel, DeleteSpendingAreaModel>(MemberList.Destination);

		CreateMap<CashVaultModel, CashVaultApiModel>(MemberList.Destination);
		CreateMap<CreateCashVaultApiModel, CreateCashVaultModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());
		CreateMap<UpdateCashVaultApiModel, UpdateCashVaultModel>(MemberList.Destination)
			.ForMember(x => x.UserId, opt => opt.Ignore());
		CreateMap<DeleteCashVaultApiModel, DeleteCashVaultModel>(MemberList.Destination);
	}
}
