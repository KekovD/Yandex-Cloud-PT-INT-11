using System.Text;
using YandexCloudVMTagChecker.Models.Interfaces;
using YandexCloudVMTagChecker.Services.Interfaces;

namespace YandexCloudVMTagChecker.Services;

public class LaunchService : ILaunchService
{
    private readonly IYandexCloudSdk _yandexCloudSdk;
    private readonly IInstanceHandler _instanceHandler;
    private readonly ILoggerStrategy _loggerStrategy;
    private readonly TimeZoneInfo _timeZoneInfo;

    public LaunchService(
        IYandexCloudSdk yandexCloudSdk,
        IInstanceHandler instanceHandler,
        ILoggerStrategy loggerStrategy,
        TimeZoneInfo timeZoneInfo)
    {
        _yandexCloudSdk = yandexCloudSdk;
        _instanceHandler = instanceHandler;
        _loggerStrategy = loggerStrategy;
        _timeZoneInfo = timeZoneInfo;
    }

    public async Task Launch(IList<string?> cloudIdList, IList<string?> folderIdList)
    {
        var logMessage = new StringBuilder();
        logMessage.AppendFormat("[{0}] <STARTED CHECKING>", TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo));
        await _loggerStrategy.LogAsync(logMessage.ToString()).ConfigureAwait(false);
        
        var clouds = _yandexCloudSdk.GetCloudsCloudIds(cloudIdList);

        foreach (var cloud in clouds)
        {
            var folders = _yandexCloudSdk.GetFolderById(folderIdList, cloud.Id);

            await _instanceHandler.CheckAndShutdownExpiredInstancesAsync(folders, cloud).ConfigureAwait(false);
        }

        logMessage.Clear();
        logMessage.AppendFormat("[{0}] <FINISHED CHECKING>", TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo));
        await _loggerStrategy.LogAsync(logMessage.ToString()).ConfigureAwait(false);
    }
}