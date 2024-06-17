using Microsoft.Extensions.DependencyInjection;
using Yandex.Cloud;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Credentials;
using YandexCloudVMTagChecker.Exceptions;
using YandexCloudVMTagChecker.Models;
using YandexCloudVMTagChecker.Models.Interfaces;
using YandexCloudVMTagChecker.Services;
using YandexCloudVMTagChecker.Services.Interfaces;

namespace YandexCloudVMTagChecker;

class Program
{
    static async Task Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration, Configuration>()
            .AddSingleton<ILoggerStrategy, ConsoleLoggerStrategy>()
            .AddSingleton<TimeZoneInfo>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var timeZoneId = config.GetTimeZoneId();
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            })
            .AddSingleton<IYandexCloudSdk>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var oAuthToken = config.GetOAuthToken() ?? throw new OAuthTokenNotFoundException();
                return new YandexCloudSdk(new Sdk(new OAuthCredentialsProvider(oAuthToken)));
            })
            .AddSingleton<InstanceService.InstanceServiceClient>(provider =>
            {
                var sdk = provider.GetRequiredService<IYandexCloudSdk>();
                return sdk.GetInstanceService();
            })
            .AddSingleton<IInstanceHandler, InstanceHandler>()
            .AddSingleton<ILaunchService, LaunchService>()
            .BuildServiceProvider();

        var launchService = serviceProvider.GetRequiredService<ILaunchService>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        var cloudIds = config.GetCloudIds();
        var folderIds = config.GetFolderIds();

        await launchService.Launch(cloudIds, folderIds);
    }
}