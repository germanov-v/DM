using System.Data;
using System.Data.Common;
using Core.Application.Abstractions;
using Core.Application.Options.Db;
using Core.Infrastructure.Persistence.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Polly;

namespace Core.Infrastructure.Persistence;

public class PgsqlConnectionFactory : IConnectionFactory<NpgsqlConnection>
{

	private readonly SemaphoreSlim _connectionLock = new(1, 1);
	private readonly DbConnectionOptions _options;
	private readonly ILogger<PgsqlConnectionFactory> _logger;
	private NpgsqlConnection? _currentConnection;
	private DbTransaction? _npgsqlTransaction;
	private bool _disposed = false;






	public PgsqlConnectionFactory(IOptions<DbConnectionOptions> options, ILogger<PgsqlConnectionFactory> logger)
	{
		_logger = logger;
		_options = options.Value;
	}

	public async Task<NpgsqlConnection> GetCurrentConnection(
		CancellationToken cancellationToken)
		=> await CreateConnection(cancellationToken);

	private async Task<NpgsqlConnection> CreateConnection(
		CancellationToken cancellationToken,
		string? aliasConnection = null)
	{


		await _connectionLock.WaitAsync(cancellationToken);

		try
		{

			if (_currentConnection == null ||
				_currentConnection.State is not (ConnectionState.Connecting or ConnectionState.Open))

			{
				await CloseAndRemoveConnection(
					cancellationToken);
				_currentConnection = await OpenNewConnection(cancellationToken);
			}
			return _currentConnection;
		}
		finally
		{
			_connectionLock.Release();
		}
	}


	public async ValueTask InitialTransaction(
		CancellationToken cancellationToken,
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
	{
		if (_disposed) throw new InvalidOperationException(UowErrorMessages.ErrorMessageCommitRollback);
		if (_npgsqlTransaction is not null) return;
		_currentConnection = await GetCurrentConnection(cancellationToken);
		_npgsqlTransaction = await _currentConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
		SetTransaction(_npgsqlTransaction);
	}


	private async Task CloseAndRemoveConnection(
		CancellationToken cancellationToken
	)
	{
		if (_currentConnection != null)
		{
			await _currentConnection.CloseAsync();
			await _currentConnection.DisposeAsync().ConfigureAwait(false);
			_currentConnection = null;
		}

	}

	private async Task<NpgsqlConnection> OpenNewConnection(
		CancellationToken cancellationToken
	)
	{

		var connection = new NpgsqlConnection(_options.ConnectionString);
		await RetryOpenConnection(connection, cancellationToken);
		return connection;
	}


	private async Task RetryOpenConnection(
		NpgsqlConnection connection,
		CancellationToken cancellationToken)
	{
		var retryPolicy = Policy
			.Handle<PostgresException>(ex =>
				ex.SqlState == PgErrorCodes.CodeErrorTooManyConnection) // Too many connections
			.WaitAndRetryAsync(new[]
				{
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(2),
					TimeSpan.FromSeconds(3),
					TimeSpan.FromSeconds(4),
					
					TimeSpan.FromSeconds(5),
					TimeSpan.FromSeconds(6),
					
					TimeSpan.FromSeconds(7),
					TimeSpan.FromSeconds(15),
					TimeSpan.FromSeconds(30),
				},
				(exception, timeSpan, retryCount, context) =>
				{

					_logger.LogWarning("Повторное подключение: попытка {RetryCount}. Ожидание {TimeSpan}. Ошибка: {Message}",
						retryCount,
						timeSpan,
						exception.Message
					);
				});

		await retryPolicy.ExecuteAsync(async () =>
			await connection.OpenAsync(cancellationToken));
	}

	public DbTransaction? CurrentTransaction => _npgsqlTransaction;

	public void SetTransaction(DbTransaction? transaction)
		=> SetCurrentTransaction(transaction);

	public async Task CommitTransaction(CancellationToken cancellationToken)
	{
		if (_npgsqlTransaction is not null)
		{
		  await	_npgsqlTransaction.CommitAsync(cancellationToken);
		  _ = _npgsqlTransaction.DisposeAsync();
		  _npgsqlTransaction = null;
		}
		else
		{
			throw new InvalidOperationException("Commit was failed! Transaction is not active.");
		}
		
	}

	public async Task RollbackTransaction(CancellationToken cancellationToken)
	{
		if (_npgsqlTransaction is not null)
		{
			await _npgsqlTransaction.RollbackAsync(cancellationToken);
			_ = _npgsqlTransaction.DisposeAsync();
			_npgsqlTransaction = null;
		}
		else
		{
			throw new InvalidOperationException("Commit was failed! Transaction is not active.");
		}
	}

	internal void SetCurrentTransaction(DbTransaction? transaction)
		=> _npgsqlTransaction = transaction;



	public async Task CloseConnections(CancellationToken cancellationToken)
	{
		await _connectionLock.WaitAsync(cancellationToken);

		try
		{

			await _currentConnection.DisposeAsync();

		}
		finally
		{

			_connectionLock.Release();

		}
	}

	public async ValueTask DisposeAsync()
	{

		await DisposeAsyncCore().ConfigureAwait(false);
		Dispose(manualCallDispose: false);
		GC.SuppressFinalize(this);

	}


	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (_disposed)
			return;

		if (_npgsqlTransaction != null)
		{
			await _npgsqlTransaction.DisposeAsync().ConfigureAwait(false);

		}

		if (_currentConnection is not null
			&& _currentConnection is IAsyncDisposable disposable)
		{
			await disposable.DisposeAsync().ConfigureAwait(false);

		}
		else
		{
			_currentConnection?.Dispose();
		}

		if (_connectionLock is not null)
		{
			_connectionLock?.Dispose();
		}



		_npgsqlTransaction = null;
		_currentConnection = null;
	}



	public void Dispose()
	{
		// DisposeAsync().AsTask().GetAwaiter().GetResult();

		// if()
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool manualCallDispose)
	{
		if (_disposed)
			return;

		if (manualCallDispose)
		{

		}


		_disposed = true;
		_npgsqlTransaction?.Dispose();
		_currentConnection?.Dispose();
		_connectionLock?.Dispose();
	}

	~PgsqlConnectionFactory()
	{
		Dispose(false);
	}
}
