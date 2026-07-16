using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Models;
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

		CreateMap<Transaction, TransactionModel>(MemberList.Destination)
			.ForMember(dest => dest.SourceAccountId, opt => opt.Ignore())
			.ForMember(dest => dest.DestinationAccountId, opt => opt.Ignore())
			.ForMember(dest => dest.Amount, opt => opt.Ignore())
			.AfterMap((src, dest) =>
			{
				var source = src.Postings.FirstOrDefault(p => p.Amount < 0);
				var destination = src.Postings.FirstOrDefault(p => p.Amount > 0);

				dest.SourceAccountId = source?.AccountId;
				dest.DestinationAccountId = destination?.AccountId;
				dest.Amount = destination?.Amount ?? (source != null ? -source.Amount : 0.0);
			});
		CreateMap<CreateTransactionModel, Transaction>(MemberList.Destination)
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.Ignore())
			.ForMember(dest => dest.User, opt => opt.Ignore())
			.ForMember(dest => dest.MadeAt, opt => opt.Ignore())
			.ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Postings, opt => opt.Ignore());

		CreateMap<CategorySpendingItem, CategorySpendingItemModel>(MemberList.Destination);
		CreateMap<CategorizedSpendingResult, SpendingCategoryModel>(MemberList.Destination);

		CreateMap<TransactionQueryModel, TransactionQuery>(MemberList.Destination);
		CreateMap<PagedResult<Transaction>, PagedResultModel<TransactionModel>>(MemberList.Destination);
	}
}
