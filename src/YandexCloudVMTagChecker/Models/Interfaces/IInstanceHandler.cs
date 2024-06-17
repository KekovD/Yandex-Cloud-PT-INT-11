using Google.Protobuf.Collections;
using Yandex.Cloud.Resourcemanager.V1;

namespace YandexCloudVMTagChecker.Models.Interfaces;

public interface IInstanceHandler
{
    Task CheckAndShutdownExpiredInstancesAsync(RepeatedField<Folder> folders, Cloud cloud);
}