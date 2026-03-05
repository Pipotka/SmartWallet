using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с денежными хранилищами
/// </summary>
public sealed class TransactionEndpointService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IMapper mapper) : ITransactionEndpointService
{
	private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
	private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;

	async Task<List<TransactionEndpointModel>> ITransactionEndpointService.GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		_ = await _userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundByIdServiceException<User>(userId);

		var transactionEndpointList = await _transactionEndpointRepository.GetListByUserIdAsync(userId, token);

		return mapper.Map<List<TransactionEndpointModel>>(transactionEndpointList);
	}

	async Task<TransactionEndpointModel> ITransactionEndpointService.CreateAsync(CreateTransactionEndpointModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		_ = await _userRepository.GetUserByIdAsync(model.UserId, token)
		    ?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);
		
		var transactionEndpoint = mapper.Map<TransactionEndpoint>(model);
		transactionEndpoint.Id = Guid.NewGuid();
		transactionEndpoint.UserId = model.UserId;
		_transactionEndpointRepository.Add(transactionEndpoint);

		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<TransactionEndpointModel>(transactionEndpoint);
	}

	async Task<TransactionEndpointModel> ITransactionEndpointService.UpdateAsync(UpdateTransactionEndpointModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		_ = await _userRepository.GetUserByIdAsync(model.UserId, token)
				?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);

		var transactionEndpoint = await _transactionEndpointRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
		    ?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(model.Id);

		mapper.Map(model, transactionEndpoint);
		_transactionEndpointRepository.Update(transactionEndpoint);

		await unitOfWork.SaveChangesAsync(token);
		return mapper.Map<TransactionEndpointModel>(transactionEndpoint);
	}

	async Task ITransactionEndpointService.DeleteAsync(DeleteTransactionEndpointModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		_ = await _userRepository.GetUserByIdAsync(model.UserId, token)
		    ?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);
		
		var transactionEndpoint = await _transactionEndpointRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
			?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(model.Id);

		_transactionEndpointRepository.Delete(transactionEndpoint);

		await unitOfWork.SaveChangesAsync(token);
	}
}