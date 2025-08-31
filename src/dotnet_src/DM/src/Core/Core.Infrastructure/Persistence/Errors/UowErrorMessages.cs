namespace Core.Infrastructure.Persistence.Errors;

public static class UowErrorMessages
{
    public const string ErrorMessageCommitRollback = "Current instance of UnitOfWork has already been commited or rolled back";

    public const string ErrorExistTransaction = "Process start transaction was failed! Exist active item of transaction";
    public const string ErrorTransactionNotStarted = "Transaction was not started";
}