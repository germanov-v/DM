using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.ValueObjects;
using Xunit.Abstractions;

namespace Core.UnitTests.Domain;

public class BaseEntityTests
{
    private readonly ITestOutputHelper _output;

    public BaseEntityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    class EntityTest : EntityRoot<IdGuid>
    {
        
    }

    [Fact]
    public void NameOfTests_ShouldReturnChildType()
    {
        var entity = new EntityTest();

        entity.Id = new IdGuid(1, Guid.NewGuid());
       
        var ex =  Assert.Throws<InvalidOperationException>(() =>
        {
            entity.Id = new IdGuid(2, Guid.NewGuid());
        });
        
        _output.WriteLine(ex.Message);
        
    }
}