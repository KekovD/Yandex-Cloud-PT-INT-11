namespace YandexCloudVMTagChecker.Exceptions;

public class TimeZoneIdNotFoundException : Exception
{
    public TimeZoneIdNotFoundException()
        : base("Time zone ID cannot be null or empty") { }
}