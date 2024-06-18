using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;

namespace YandexCloudVMTagChecker.Models.Interfaces;

public interface IInstanceExpirationChecker
{
    Task<bool> HasExpired(Instance instance, Folder folder, Cloud cloud);
}