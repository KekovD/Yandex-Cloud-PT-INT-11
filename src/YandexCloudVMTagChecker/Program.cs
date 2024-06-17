using YandexCloudVMTagChecker.Exceptions;
using YandexCloudVMTagChecker.Models;
using YandexCloudVMTagChecker.Services;

namespace YandexCloudVMTagChecker;

public class Program
{
    static async Task Main(string[] args)
    {
        var logger = new ConsoleLoggerStrategy();
        
        var configuration = new Configuration();
        string oauthToken = configuration.GetOAuthToken() ?? throw new OAuthTokenNotFoundException();
        IList<string?> folderIdList = configuration.GetFolderIds();
        IList<string?> cloudIdList = configuration.GetCloudIds();

        var yandexCloudSdk = new YandexCloudSdk(oauthToken);
        var instanceService = yandexCloudSdk.GetInstanceService();
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        var instanceHandler = new InstanceHandler(logger, instanceService, timeZoneInfo);
        var launchService = new LaunchService(yandexCloudSdk, instanceHandler);

        await launchService.Launch(cloudIdList, folderIdList).ConfigureAwait(false);
    }
}