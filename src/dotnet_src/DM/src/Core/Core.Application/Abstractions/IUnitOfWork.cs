using System.Data;

namespace Core.Application.Abstractions;

public interface IUnitOfWork
{
    
    ValueTask StartTransaction(CancellationToken cancellationToken, 
         IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    
    
    
    ValueTask CommitTransaction(CancellationToken cancellationToken);
    
    ValueTask RollbackTransaction(CancellationToken cancellationToken);
    
    
    ValueTask RollbackTransactionIfExist(CancellationToken cancellationToken);
}