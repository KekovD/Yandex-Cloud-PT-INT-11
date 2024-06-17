using Google.Protobuf.Collections;
using Yandex.Cloud;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace YandexCloudVMTagChecker.Models;

public class YandexCloudSdk : IYandexCloudSdk
{
    private readonly Sdk _sdk;

    public YandexCloudSdk(Sdk sdk)
    {
        _sdk = sdk;
    }

    public InstanceService.InstanceServiceClient GetInstanceService() => _sdk.Services.Compute.InstanceService;

    public RepeatedField<Folder> GetFolderById(IList<string?> folderIds, string cloudId)
    {
        var folders =
            _sdk.Services.Resourcemanager.FolderService.List(new ListFoldersRequest { CloudId = cloudId }).Folders;

        var filteredFolders = new RepeatedField<Folder>();

        foreach (var folder in folders.Where(folder => folderIds.Contains(folder.Id)))
        {
            filteredFolders.Add(folder);
        }

        return filteredFolders;
    }

    public RepeatedField<Cloud> GetCloudsCloudIds(IList<string?> cloudIds)
    {
        var cloudNameAndIds = new RepeatedField<Cloud>();


        var cloudService = _sdk.Services.Resourcemanager.CloudService;
        var getCloudList = cloudService.List(new ListCloudsRequest());
        var clouds = getCloudList.Clouds;

        foreach (var cloud in clouds.Where(cloud => cloudIds.Contains(cloud.Id)))
        {
            cloudNameAndIds.Add(cloud);
        }

        return cloudNameAndIds;
    }
}