namespace Core.Application.Abstractions;

public interface IUnitOfWork
{
    ValueTask Commit(CancellationToken cancellationToken);
}