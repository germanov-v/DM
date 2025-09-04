using System.Data;
using System.Data.Common;

namespace Core.Infrastructure.Persistence;

public interface IConnectionFactory<TConnection> : IAsyncDisposable, IDisposable
{

    ValueTask InitialTransaction(
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    
    Task<TConnection> GetCurrentConnection(CancellationToken cancellationToken);
    DbTransaction? CurrentTransaction { get; }
    
    Task CommitTransaction(
        CancellationToken cancellationToken);



    Task RollbackTransaction(CancellationToken cancellationToken);
}