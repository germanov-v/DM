namespace Core.UnitTests.Domain.Types;

public class DateTimeOffsetTests
{
    [Fact]
    public void DateTimeOffset_ShouldNotThrowNotException()
    {
        var now = DateTimeOffset.Now;
        var server =DateTimeOffset.Now;
        var  utcNow = now.UtcDateTime;
       
    }
}