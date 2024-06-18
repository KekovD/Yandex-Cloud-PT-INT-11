using Yandex.Cloud.Compute.V1;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace YandexCloudVMTagChecker.Models;

public class InstanceClientWrapper : IInstanceClientWrapper
{
    private readonly InstanceService.InstanceServiceClient _innerClient;

    public InstanceClientWrapper(InstanceService.InstanceServiceClient innerClient)
    {
        _innerClient = innerClient;
    }

    public async Task<ListInstancesResponse> ListAsync(ListInstancesRequest request)
    {
        return await _innerClient.ListAsync(request).ConfigureAwait(false);
    }

    public async Task StopAsync(StopInstanceRequest request)
    {
        await _innerClient.StopAsync(request).ConfigureAwait(false);
    }
}