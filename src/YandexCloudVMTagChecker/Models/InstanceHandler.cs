using System.Text;
using Google.Protobuf.Collections;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace YandexCloudVMTagChecker.Models;

public class InstanceHandler : IInstanceHandler
{
    private readonly ILoggerStrategy _loggerStrategy;
    private readonly IInstanceClientWrapper _instanceClientWrapper;
    private readonly IInstanceExpirationChecker _expirationChecker;
    private readonly TimeZoneInfo _timeZoneInfo;

    public InstanceHandler(
        ILoggerStrategy loggerStrategy,
        IInstanceClientWrapper instanceClientWrapper,
        IInstanceExpirationChecker expirationChecker,
        TimeZoneInfo timeZoneInfo)
    {
        _loggerStrategy = loggerStrategy;
        _instanceClientWrapper = instanceClientWrapper;
        _expirationChecker = expirationChecker;
        _timeZoneInfo = timeZoneInfo;
    }

    public async Task CheckAndShutdownExpiredInstancesAsync(RepeatedField<Folder> folders, Cloud cloud)
    {
        var logMessage = new StringBuilder();

        logMessage.AppendFormat("<+++Start work in the cloud {0} (ID: {1})", cloud.Name, cloud.Id);

        await LogAsync(logMessage.ToString());

        foreach (var folder in folders)
        {
            logMessage.Clear();

            logMessage.AppendFormat("<+++++Start work in the folder {0} (ID: {1})", folder.Name, folder.Id);

            await LogAsync(logMessage.ToString());

            var instancesResponse =
                await _instanceClientWrapper.ListAsync(new ListInstancesRequest { FolderId = folder.Id })
                    .ConfigureAwait(false);

            foreach (var instance in instancesResponse.Instances)
            {
                await CheckAndShutdownInstanceAsync(instance, folder, cloud).ConfigureAwait(false);
            }

            logMessage.Clear();

            logMessage.AppendFormat("<-----End work in the folder {0} (ID: {1})", folder.Name, folder.Id);

            await LogAsync(logMessage.ToString());
        }

        logMessage.Clear();

        logMessage.AppendFormat("<---End work in the cloud {0} (ID: {1})", cloud.Name, cloud.Id);

        await LogAsync(logMessage.ToString());
    }

    private async Task CheckAndShutdownInstanceAsync(Instance instance, Folder folder, Cloud cloud)
    {
        if (await _expirationChecker.HasExpired(instance, folder, cloud).ConfigureAwait(false) &&
            instance.Status == Instance.Types.Status.Running)
        {
            var logMessage = new StringBuilder();
            logMessage.AppendFormat(
                "[INFO] Cloud {0} (ID: {1}) folder {2} (ID: {3}) VM {4} (ID: {5}) has expired. Shutting down...",
                cloud.Name, cloud.Id, folder.Name, folder.Id, instance.Name, instance.Id);

            await LogAsync(logMessage.ToString());

            await _instanceClientWrapper.StopAsync(new StopInstanceRequest { InstanceId = instance.Id })
                .ConfigureAwait(false);
        }
    }

    private async Task LogAsync(string message)
    {
        var formattedMessage = $"[{TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo)}] {message}";
        await _loggerStrategy.LogAsync(formattedMessage).ConfigureAwait(false);
    }
}