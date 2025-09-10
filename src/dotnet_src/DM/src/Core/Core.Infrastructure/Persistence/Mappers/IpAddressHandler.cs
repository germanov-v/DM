using System.Data;
using System.Net;
using Dapper;

namespace Core.Infrastructure.Persistence.Mappers;

public sealed class IpAddressHandler : SqlMapper.TypeHandler<IPAddress>
{
    public override void SetValue(IDbDataParameter parameter, IPAddress? value)
    {
        parameter.Value = value;               
        parameter.DbType = DbType.Object;    
    }

    public override IPAddress Parse(object value)
    {
        return (IPAddress)value;                
    }
}