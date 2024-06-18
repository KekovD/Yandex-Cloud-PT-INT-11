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
            .AddTransient<IConfiguration, Configuration>()
            .AddTransient<ILoggerStrategy, ConsoleLoggerStrategy>()
            .AddTransient<TimeZoneInfo>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var timeZoneId = config.GetTimeZoneId();
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            })
            .AddTransient<IYandexCloudSdk>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var oAuthToken = config.GetOAuthToken() ?? throw new OAuthTokenNotFoundException();
                return new YandexCloudSdk(new Sdk(new OAuthCredentialsProvider(oAuthToken)));
            })
            .AddTransient<IInstanceExpirationChecker, InstanceExpirationChecker>()
            .AddTransient<IInstanceClientWrapper, InstanceClientWrapper>()
            .AddTransient<InstanceService.InstanceServiceClient>(provider =>
            {
                var sdk = provider.GetRequiredService<IYandexCloudSdk>();
                return sdk.GetInstanceService();
            })
            .AddTransient<IInstanceHandler, InstanceHandler>()
            .AddTransient<ILaunchService, LaunchService>()
            .BuildServiceProvider();

        var launchService = serviceProvider.GetRequiredService<ILaunchService>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        var cloudIds = config.GetCloudIds();
        var folderIds = config.GetFolderIds();

        await launchService.Launch(cloudIds, folderIds);
    }
}