using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Core.Domain.SharedKernel.Exceptions;

public static class ThrowHelper
{
    [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
    public static void IdAlreadySet(string typeName, string propertyName, string keyType)
        =>
            throw new InvalidOperationException(
                $"{typeName} property {propertyName} with type {keyType} can only be set once.");
    
}