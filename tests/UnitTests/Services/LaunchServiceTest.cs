using Google.Protobuf.Collections;
using Moq;
using Yandex.Cloud.Resourcemanager.V1;
using YandexCloudVMTagChecker.Models.Interfaces;
using YandexCloudVMTagChecker.Services;

namespace UnitTests.Services;

[TestSubject(typeof(LaunchService))]
public class LaunchServiceTest
{
    [Fact]
    public async Task Launch_Should_Log_Start_And_Finish_Messages()
    {
        var mockYandexCloudSdk = new Mock<IYandexCloudSdk>();
        var mockInstanceHandler = new Mock<IInstanceHandler>();
        var mockLoggerStrategy = new Mock<ILoggerStrategy>();
        var mockTimeZoneInfo = TimeZoneInfo.Utc;

        var launchService = new LaunchService(
            mockYandexCloudSdk.Object,
            mockInstanceHandler.Object,
            mockLoggerStrategy.Object,
            mockTimeZoneInfo
        );

        var cloudIdList = new List<string?> { "cloud1", "cloud2" };
        var folderIdList = new List<string?> { "folder1", "folder2" };

        var capturedLogs = new List<string>();

        mockLoggerStrategy.Setup(l => l.LogAsync(It.IsAny<string>()))
            .Callback<string>(logMessage => capturedLogs.Add(logMessage))
            .Returns(Task.CompletedTask);
        
        var cloud1 = new Cloud { Id = "cloud1" };
        var cloud2 = new Cloud { Id = "cloud2" };
        var clouds = new RepeatedField<Cloud> { cloud1, cloud2 };

        mockYandexCloudSdk.Setup(sdk => sdk.GetCloudsCloudIds(cloudIdList))
            .Returns(clouds);

        var folder1 = new Folder { Id = "folder1" };
        var folder2 = new Folder { Id = "folder2" };
        var foldersCloud1 = new RepeatedField<Folder> { folder1 };
        var foldersCloud2 = new RepeatedField<Folder> { folder2 };

        mockYandexCloudSdk.Setup(sdk => sdk.GetFolderById(folderIdList, "cloud1"))
            .Returns(foldersCloud1);

        mockYandexCloudSdk.Setup(sdk => sdk.GetFolderById(folderIdList, "cloud2"))
            .Returns(foldersCloud2);
        
        await launchService.Launch(cloudIdList, folderIdList);

        Assert.Equal(2, capturedLogs.Count);

        Assert.Contains("<STARTED CHECKING>", capturedLogs[0]);

        Assert.Contains("FINISHED CHECKING>", capturedLogs[1]);

        mockYandexCloudSdk.Verify(sdk => sdk.GetCloudsCloudIds(cloudIdList), Times.Once);
        mockYandexCloudSdk.Verify(sdk => sdk.GetFolderById(folderIdList, "cloud1"), Times.Once);
        mockYandexCloudSdk.Verify(sdk => sdk.GetFolderById(folderIdList, "cloud2"), Times.Once);
    }
}