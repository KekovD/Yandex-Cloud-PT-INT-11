using System.Collections;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace YandexCloudVMTagChecker.Models;

public class Configuration : IConfiguration
{
    public string? GetOAuthToken() => Environment.GetEnvironmentVariable("OAUTH_TOKEN");

    public IList<string?> GetCloudIds() => GetEnvironmentValuesByPattern("CLOUD_ID_");

    public IList<string?> GetFolderIds() => GetEnvironmentValuesByPattern("FOLDER_ID_");
    public string GetTimeZoneId() => Environment.GetEnvironmentVariable("TIME_ZONE_PROGRAM") ?? "UTC";

    private static IList<string?> GetEnvironmentValuesByPattern(string pattern) =>
        Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .Where(e => ((string)e.Key)
                .StartsWith(pattern))
            .Select(e => (string?)e.Value)
            .ToList();
}