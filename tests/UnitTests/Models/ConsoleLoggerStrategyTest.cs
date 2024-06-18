using YandexCloudVMTagChecker.Models;

namespace UnitTests.Models;

[TestSubject(typeof(ConsoleLoggerStrategy))]
public class ConsoleLoggerStrategyTest
{
    [Fact]
    public async Task LogAsync_WritesLogToConsole()
    {
        var logger = new ConsoleLoggerStrategy();
        var expectedLog = "This is a test log.";

        await using var sw = new StringWriter();
        
        Console.SetOut(sw);

        await logger.LogAsync(expectedLog);

        var result = sw.ToString().Trim();
        Assert.Equal(expectedLog, result);
    }
}