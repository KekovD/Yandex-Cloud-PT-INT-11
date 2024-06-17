namespace YandexCloudVMTagChecker.Models.Interfaces;

public interface ILoggerStrategy
{
    Task LogAsync(string log);
}