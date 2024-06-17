using YandexCloudVMTagChecker.Models.Interfaces;

namespace YandexCloudVMTagChecker.Models;

public class ConsoleLoggerStrategy : ILoggerStrategy
{
    public Task LogAsync(string log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}