using System.Globalization;
using Yandex.Cloud;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Credentials;

namespace YandexCloudVMTagChecker;

public class Program
{
    static async Task Main(string[] args)
    {
        var oauthToken = Environment.GetEnvironmentVariable("OAUTH_TOKEN");
        var folderId = Environment.GetEnvironmentVariable("FOLDER_ID");

        var sdk = new Sdk(new OAuthCredentialsProvider(oauthToken));
        var instanceService = sdk.Services.Compute.InstanceService;

        var instancesResponse = await instanceService.ListAsync(new ListInstancesRequest
        {
            FolderId = folderId
        });

        foreach (var instance in instancesResponse.Instances)
        {
            if (instance.Labels.TryGetValue("expired_date", out string expiredDateStr) &&
                DateTime.TryParseExact(expiredDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTime expiredDate))
            {
                if (expiredDate <= DateTime.Today)
                {
                    Console.WriteLine($"VM {instance.Name} (ID: {instance.Id}) has expired. Shutting down...");

                    await instanceService.StopAsync(new StopInstanceRequest
                    {
                        InstanceId = instance.Id
                    });
                }
            }
        }
    }
}