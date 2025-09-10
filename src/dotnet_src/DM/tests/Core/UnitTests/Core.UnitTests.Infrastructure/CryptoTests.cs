using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace Core.UnitTests.Infrastructure;

public class CryptoTests
{
    private readonly ITestOutputHelper _output;

    public CryptoTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void GenerateCryptoKey_ShouldNotThrowException()
    {
        var keyBytes = RandomNumberGenerator.GetBytes(512 / 8);
        var key = new SymmetricSecurityKey(keyBytes);
        var keyStr = Convert.ToBase64String(key.Key);
        
        
         _output.WriteLine(keyStr);
    }
}