using System.Data;
using System.Data.Common;

namespace Core.Application.Abstractions;

public interface IDbConnectionFactory<TConnection> : IAsyncDisposable, IDisposable
{

    Task<TConnection> GetCurrentConnection(CancellationToken cancellationToken);
    DbTransaction? CurrentTransaction { get; }
}