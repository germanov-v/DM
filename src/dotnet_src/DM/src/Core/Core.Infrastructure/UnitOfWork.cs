using System.Data;
using Core.Application.Abstractions;

namespace Core.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    public ValueTask StartTransaction(CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotImplementedException();
    }

    public ValueTask CommitTransaction(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask RollbackTransaction(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask RollbackTransactionIfExist(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}