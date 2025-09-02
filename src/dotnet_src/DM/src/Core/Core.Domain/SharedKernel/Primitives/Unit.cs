namespace Core.Domain.SharedKernel.Primitives;

/// <summary>
/// На сколько он вообще нужен
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    public bool Equals(Unit other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return obj is Unit other && Equals(other);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}