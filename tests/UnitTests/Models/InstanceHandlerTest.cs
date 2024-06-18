using Google.Protobuf.Collections;
using Moq;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;
using YandexCloudVMTagChecker.Models;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace UnitTests.Models;

[TestSubject(typeof(InstanceHandler))]
public class InstanceHandlerTest
{
    private readonly Mock<ILoggerStrategy> _loggerStrategyMock;
    private readonly Mock<IInstanceClientWrapper> _instanceClientWrapperMock;
    private readonly Mock<IInstanceExpirationChecker> _expirationCheckerMock;
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly InstanceHandler _instanceHandler;

    public InstanceHandlerTest()
    {
        _loggerStrategyMock = new Mock<ILoggerStrategy>();
        _instanceClientWrapperMock = new Mock<IInstanceClientWrapper>();
        _expirationCheckerMock = new Mock<IInstanceExpirationChecker>();
        _timeZoneInfo = TimeZoneInfo.Utc;

        _instanceHandler = new InstanceHandler(
            _loggerStrategyMock.Object,
            _instanceClientWrapperMock.Object,
            _expirationCheckerMock.Object,
            _timeZoneInfo);
    }

    [Fact]
    public async Task CheckAndShutdownExpiredInstancesAsync_ExpiredInstance_ShouldLogAndStopInstance()
    {
        var folder = new Folder { Id = "folder1", Name = "TestFolder" };
        var folders = new RepeatedField<Folder> { folder };
        var cloud = new Cloud { Id = "cloud1", Name = "TestCloud" };
        var instance = new Instance { Id = "instance1", Name = "TestInstance", Status = Instance.Types.Status.Running };
        var instancesResponse = new ListInstancesResponse { Instances = { instance } };

        _instanceClientWrapperMock
            .Setup(x => x.ListAsync(It.IsAny<ListInstancesRequest>()))
            .ReturnsAsync(instancesResponse);

        _expirationCheckerMock
            .Setup(x =>
                x.HasExpired(It.IsAny<Instance>(), It.IsAny<Folder>(), It.IsAny<Cloud>()))
            .ReturnsAsync(true);

        _instanceClientWrapperMock
            .Setup(x => x.StopAsync(It.IsAny<StopInstanceRequest>()))
            .Returns(Task.CompletedTask);

        await _instanceHandler.CheckAndShutdownExpiredInstancesAsync(folders, cloud);

        _loggerStrategyMock.Verify(x =>
                x.LogAsync(It.Is<string>(s => s.Contains("has expired. Shutting down..."))),
            Times.Once);
        _instanceClientWrapperMock.Verify(
            x =>
                x.StopAsync(It.Is<StopInstanceRequest>(req =>
                    req.InstanceId == "instance1")), Times.Once);
    }

    [Fact]
    public async Task CheckAndShutdownExpiredInstancesAsync_NotExpiredInstance_ShouldNotStopInstance()
    {
        var folder = new Folder { Id = "folder1", Name = "TestFolder" };
        var folders = new RepeatedField<Folder> { folder };
        var cloud = new Cloud { Id = "cloud1", Name = "TestCloud" };
        var instance = new Instance { Id = "instance1", Name = "TestInstance", Status = Instance.Types.Status.Running };
        var instancesResponse = new ListInstancesResponse { Instances = { instance } };

        _instanceClientWrapperMock
            .Setup(x => x.ListAsync(It.IsAny<ListInstancesRequest>()))
            .ReturnsAsync(instancesResponse);

        _expirationCheckerMock
            .Setup(x =>
                x.HasExpired(It.IsAny<Instance>(), It.IsAny<Folder>(), It.IsAny<Cloud>()))
            .ReturnsAsync(false);

        await _instanceHandler.CheckAndShutdownExpiredInstancesAsync(folders, cloud);

        _loggerStrategyMock.Verify(x => x.LogAsync(It.IsAny<string>()),
            Times.Exactly(4));
        _instanceClientWrapperMock.Verify(x =>
            x.StopAsync(It.IsAny<StopInstanceRequest>()), Times.Never);
    }
}