using DaprSaga.Shared.Models;

namespace DaprSaga.Shared.Interfaces;

public interface ITransactionService
{
    // Forward transaction
    Task<TransactionResult> Deposit(SharedTransactionRequest payload);
    Task<TransactionResult> Withdraw(SharedTransactionRequest payload);
    
    // Compensation
    Task CompensateDeposit(string transactionId);
    Task CompensateWithdraw(string transactionId);
}
