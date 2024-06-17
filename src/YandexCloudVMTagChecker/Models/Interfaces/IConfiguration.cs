namespace YandexCloudVMTagChecker.Models.Interfaces;

public interface IConfiguration
{
    string? GetOAuthToken();
    IList<string?> GetCloudIds();
    IList<string?> GetFolderIds();
    string? GetTimeZoneId();
}