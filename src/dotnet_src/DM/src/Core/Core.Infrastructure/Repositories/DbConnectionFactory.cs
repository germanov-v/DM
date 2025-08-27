using System.Data.Common;
using Core.Application.Abstractions;
using Npgsql;

namespace Core.Infrastructure.Repositories;

public class DbConnectionFactory : IDbConnectionFactory<NpgsqlConnection>
{
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }

    public Task<NpgsqlConnection> GetCurrentConnection(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public DbTransaction? CurrentTransaction { get; }
}