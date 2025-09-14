using Core.Domain.SharedKernel.ValueObjects;

namespace Core.UnitTests.Domain.Types;

public class ValueObjectsTests
{
    [Fact]
    public void StatusValue_ShouldSetValue()
    {
        var data = new Status(true, DateTimeOffset.Now);
        
        Assert.True(data.Value);
    }
    
    [Fact]
    public void AppDate_ShouldSetValue()
    {
        var date = DateTimeOffset.Now;
        var data = new AppDate(date.AddSeconds(1));
        
        Assert.True(data.Value>date);
    }
    [Fact]
    public void DateTimeOffset_ShouldSetValue()
    {
        var date = DateTimeOffset.Now;
        var data = date.AddSeconds(1);
        
        Assert.True(data>date);
    }
}