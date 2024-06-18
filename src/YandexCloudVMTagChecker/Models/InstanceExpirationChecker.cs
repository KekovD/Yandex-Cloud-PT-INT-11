using System.Globalization;
using System.Text;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace YandexCloudVMTagChecker.Models;

public class InstanceExpirationChecker : IInstanceExpirationChecker
{
    private readonly ILoggerStrategy _loggerStrategy;
    private readonly TimeZoneInfo _timeZoneInfo;

    public InstanceExpirationChecker(ILoggerStrategy loggerStrategy, TimeZoneInfo timeZoneInfo)
    {
        _loggerStrategy = loggerStrategy;
        _timeZoneInfo = timeZoneInfo;
    }

    public async Task<bool> HasExpired(Instance instance, Folder folder, Cloud cloud)
    {
        var expiredDate = GetExpiredDateFromLabel(instance);

        if (expiredDate.HasValue)
        {
            return CheckIfExpired(expiredDate.Value);
        }

        var message = CreateErrorMessage(instance, folder, cloud, expiredDate.HasValue);
        await _loggerStrategy.LogAsync(message);

        return false;
    }

    private DateTime? GetExpiredDateFromLabel(Instance instance)
    {
        if (instance.Labels.TryGetValue("expired_date", out string expiredDateStr) &&
            DateTime.TryParseExact(
                expiredDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiredDate))
        {
            return TimeZoneInfo.ConvertTime(expiredDate, _timeZoneInfo).Date;
        }

        return null;
    }

    private bool CheckIfExpired(DateTime expiredDate)
    {
        var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo);
        var currentDate = currentTime.Date;
        return expiredDate < currentDate;
    }

    private string CreateErrorMessage(Instance instance, Folder folder, Cloud cloud, bool hasValidDate)
    {
        var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo);
        var message = new StringBuilder();
        
        message.AppendFormat("[{0}] [WARNING] Cloud {1} (ID: {2}) folder {3} (ID: {4}) VM {5} (ID: {6}). ",
            currentTime, cloud.Name, cloud.Id, folder.Name, folder.Id, instance.Name, instance.Id);

        message.Append(hasValidDate
            ? "The date is in an incorrect format. Use \"dd.MM.yyyy\""
            : "Label \"expired_date\" is missing");

        return message.ToString();
    }
}
