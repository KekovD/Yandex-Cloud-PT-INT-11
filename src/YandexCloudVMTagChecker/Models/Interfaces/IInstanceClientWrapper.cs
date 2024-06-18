using Yandex.Cloud.Compute.V1;

namespace YandexCloudVMTagChecker.Models.Interfaces;

public interface IInstanceClientWrapper
{
    Task<ListInstancesResponse> ListAsync(ListInstancesRequest request);
    Task StopAsync(StopInstanceRequest request);
}