using Google.Protobuf.Collections;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;

namespace YandexCloudVMTagChecker.Models.Interfaces;

public interface IYandexCloudSdk
{
    InstanceService.InstanceServiceClient GetInstanceService();
    RepeatedField<Folder> GetFolderById(IList<string?> folderIds, string cloudId);
    RepeatedField<Cloud> GetCloudsCloudIds(IList<string?> cloudIds);
}