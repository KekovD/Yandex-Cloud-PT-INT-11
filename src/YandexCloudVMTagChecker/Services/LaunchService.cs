using YandexCloudVMTagChecker.Models.Interfaces;
using YandexCloudVMTagChecker.Services.Interfaces;

namespace YandexCloudVMTagChecker.Services;

public class LaunchService : ILaunchService
{
    private readonly IYandexCloudSdk _yandexCloudSdk;
    private readonly IInstanceHandler _instanceHandler;

    public LaunchService(IYandexCloudSdk yandexCloudSdk, IInstanceHandler instanceHandler)
    {
        _yandexCloudSdk = yandexCloudSdk;
        _instanceHandler = instanceHandler;
    }

    public async Task Launch(IList<string?> cloudIdList, IList<string?> folderIdList)
    {
        var clouds = _yandexCloudSdk.GetCloudsCloudIds(cloudIdList);

        foreach (var cloud in clouds)
        {
            var folders = _yandexCloudSdk.GetFolderById(folderIdList, cloud.Id);

            await _instanceHandler.CheckAndShutdownExpiredInstancesAsync(folders, cloud).ConfigureAwait(false);
        }
    }
}