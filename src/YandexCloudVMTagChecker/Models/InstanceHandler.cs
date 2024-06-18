using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Google.Protobuf.Collections;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace YandexCloudVMTagChecker.Models;

public class InstanceHandler : IInstanceHandler
{
    private readonly ILoggerStrategy _loggerStrategy;
    private readonly InstanceService.InstanceServiceClient _instanceService;
    private readonly TimeZoneInfo _timeZoneInfo;

    public InstanceHandler(
        ILoggerStrategy loggerStrategy,
        InstanceService.InstanceServiceClient instanceService,
        TimeZoneInfo timeZoneInfo)
    {
        _loggerStrategy = loggerStrategy;
        _instanceService = instanceService;
        _timeZoneInfo = timeZoneInfo;
    }

    public async Task CheckAndShutdownExpiredInstancesAsync(RepeatedField<Folder> folders, Cloud cloud)
    {
        var message = new StringBuilder();
        
        message.AppendFormat("[{0}] <+++Start work in the cloud {1} (ID: {2})",
            TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo), cloud.Name, cloud.Id);

        await _loggerStrategy.LogAsync(message.ToString()).ConfigureAwait(false);
        message.Clear();

        foreach (var folder in folders)
        {
            message
                .AppendFormat("[{0}] <+++++Start work in the folder {1} (ID: {2})",
                    TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo), folder.Name, folder.Id);

            await _loggerStrategy.LogAsync(message.ToString()).ConfigureAwait(false);
            message.Clear();
            
            var instancesResponse = await GetInstancesAsync(folder).ConfigureAwait(false);

            foreach (var instance in instancesResponse.Instances)
            {
                await CheckAndShutdownInstanceAsync(instance, folder, cloud).ConfigureAwait(false);
            }

            message
                .AppendFormat("[{0}] <-----End work in the folder {1} (ID: {2})",
                    TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo), folder.Name, folder.Id);

            await _loggerStrategy.LogAsync(message.ToString()).ConfigureAwait(false);
            message.Clear();
        }

        message.AppendFormat("[{0}] <---End work in the cloud {1} (ID: {2})",
            TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo), cloud.Name, cloud.Id);

        await _loggerStrategy.LogAsync(message.ToString()).ConfigureAwait(false);
    }

    private async Task<ListInstancesResponse> GetInstancesAsync(Folder folder)
    {
        return await _instanceService.ListAsync(new ListInstancesRequest
        {
            FolderId = folder.Id
        }).ConfigureAwait(false);
    }

    private async Task CheckAndShutdownInstanceAsync(Instance instance, Folder folder, Cloud cloud)
    {
        if (await HasExpired(instance, folder, cloud).ConfigureAwait(false) &&
            instance.Status == Instance.Types.Status.Running)
        {
            var message = new StringBuilder();

            message.AppendFormat(
                "[{0}] [INFO] Cloud {1} (ID: {2}) folder {3} (ID: {4}) VM {5} (ID: {6}) has expired. Shutting down...",
                TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo),
                cloud.Name,
                cloud.Id,
                folder.Name,
                folder.Id,
                instance.Name,
                instance.Id);

            await _loggerStrategy.LogAsync(message.ToString()).ConfigureAwait(false);

            await _instanceService.StopAsync(new StopInstanceRequest
            {
                InstanceId = instance.Id
            }).ConfigureAwait(false);
        }
    }


    private async Task<bool> HasExpired(Instance instance, Folder folder, Cloud cloud)
    {
        var tryGetValue = instance.Labels.TryGetValue("expired_date", out string expiredDateStr);
        
        var tryParseExact = DateTime.TryParseExact(
            expiredDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiredDate);

        var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo);
        var currentDate = currentTime.Date;
        
    
        if (tryGetValue && tryParseExact)
        {
            expiredDate = TimeZoneInfo.ConvertTime(expiredDate, _timeZoneInfo).Date;
            return expiredDate < currentDate;
        }

        var message = new StringBuilder();

        message
            .AppendFormat("[{0}] [WARNING] Cloud {1} (ID: {2}) folder {3} (ID: {4}) VM {5} (ID: {6}). ",
                currentTime, cloud.Name, cloud.Id, folder.Name, folder.Id, instance.Name, instance.Id);

        message
            .Append(!tryGetValue
                ? "Label \"expired_date\" is missing"
                : "The date is in an incorrect format. Use \"dd.MM.yyyy\"");

        await _loggerStrategy.LogAsync(message.ToString()).ConfigureAwait(false);

        return false;
    }
}