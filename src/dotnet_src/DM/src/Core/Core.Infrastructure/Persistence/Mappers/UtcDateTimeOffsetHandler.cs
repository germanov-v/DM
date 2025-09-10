using System.Data;
using Dapper;

namespace Core.Infrastructure.Persistence.Mappers;

public sealed class UtcDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    private readonly TimeSpan? _readOffset; 

    public UtcDateTimeOffsetHandler(TimeSpan? readOffset = null)
        => _readOffset = readOffset;

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        // timestamptz требует UTC
        var utc = value.ToUniversalTime();         
        parameter.Value = utc;       
    }

    public override DateTimeOffset Parse(object value)
    {
        DateTimeOffset dto = value switch
        {
            DateTimeOffset d => d,                    
            DateTime dt      => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
            _                => (DateTimeOffset)value
        };

       
        return _readOffset is { } off ? dto.ToOffset(off) : dto;
    }
}