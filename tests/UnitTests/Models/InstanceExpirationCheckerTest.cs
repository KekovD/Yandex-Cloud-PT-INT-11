using System.Reflection;
using Google.Protobuf.Collections;
using Moq;
using Yandex.Cloud.Compute.V1;
using Yandex.Cloud.Resourcemanager.V1;
using YandexCloudVMTagChecker.Models;
using YandexCloudVMTagChecker.Models.Interfaces;

namespace UnitTests.Models;

[TestSubject(typeof(InstanceExpirationChecker))]
public class InstanceExpirationCheckerTest
{
    private readonly Mock<ILoggerStrategy> _loggerMock = new Mock<ILoggerStrategy>();

    private readonly InstanceExpirationChecker _checker;

    public InstanceExpirationCheckerTest()
    {
        TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;
        _checker = new InstanceExpirationChecker(_loggerMock.Object, utcTimeZone);
    }

    public static Instance CreateInstanceWithLabels(Dictionary<string, string> labels)
    {
        var instance = new Instance();
        var labelsField = typeof(Instance).GetField("labels_", BindingFlags.Instance | BindingFlags.NonPublic);

        if (labelsField is null) return instance;

        var mapField = new MapField<string, string>();
        foreach (var label in labels)
        {
            mapField.Add(label.Key, label.Value);
        }

        labelsField.SetValue(instance, mapField);

        return instance;
    }

    [Fact]
    public async Task HasExpired_ExpiredInstance_ReturnsTrue()
    {
        var instance = CreateInstanceWithLabels(new Dictionary<string, string>
        {
            { "expired_date", DateTime.UtcNow.AddDays(-1).ToString("dd.MM.yyyy") }
        });

        var folder = new Folder();
        var cloud = new Cloud();

        var result = await _checker.HasExpired(instance, folder, cloud);

        Assert.True(result);
        _loggerMock.Verify(
            x => x.LogAsync(It.IsAny<string>()),
            Times.Never(),
            "Logger should not be called when instance has expired."
        );
    }

    [Fact]
    public async Task HasExpired_NonExpiredInstance_ReturnsFalse()
    {
        var instance = CreateInstanceWithLabels(new Dictionary<string, string>
        {
            { "expired_date", DateTime.UtcNow.AddDays(1).ToString("dd.MM.yyyy") }
        });

        var folder = new Folder();
        var cloud = new Cloud();

        var result = await _checker.HasExpired(instance, folder, cloud);

        Assert.False(result);
        _loggerMock.Verify(
            x => x.LogAsync(It.IsAny<string>()),
            Times.Never(),
            "Logger should not be called when instance has not expired."
        );
    }

    [Fact]
    public async Task HasExpired_MissingLabel_ReturnsFalseAndLogsError()
    {
        var instance = CreateInstanceWithLabels(new Dictionary<string, string>
        {
            { "wrong_label", DateTime.UtcNow.AddDays(-1).ToString("dd.MM.yyyy") }
        }); 

        var folder = new Folder();
        var cloud = new Cloud();

        // Act
        var result = await _checker.HasExpired(instance, folder, cloud);

        // Assert
        Assert.False(result);
        _loggerMock.Verify(
            x => x.LogAsync(It.IsAny<string>()),
            Times.Once(),
            "Logger should be called once for missing label."
        );
    }

    [Fact]
    public async Task HasExpired_InvalidDateFormat_ReturnsFalseAndLogsError()
    {
        var instance = CreateInstanceWithLabels(new Dictionary<string, string>
        {
            { "expired_date", "18.06" }
        });

        var folder = new Folder();
        var cloud = new Cloud();

        var result = await _checker.HasExpired(instance, folder, cloud);

        Assert.False(result);
        _loggerMock.Verify(
            x => x.LogAsync(It.IsAny<string>()),
            Times.Once(),
            "Logger should be called once for invalid date format."
        );
    }
}