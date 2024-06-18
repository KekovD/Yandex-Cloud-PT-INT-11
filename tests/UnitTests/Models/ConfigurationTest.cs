using YandexCloudVMTagChecker.Models;

namespace UnitTests.Models;

[TestSubject(typeof(Configuration))]
public class ConfigurationTest
{
       [Fact]
        public void GetOAuthToken_ReturnsCorrectValue()
        {
            Environment.SetEnvironmentVariable("OAUTH_TOKEN", null);
            
            var expectedValue = "test-oauth-token";
            Environment.SetEnvironmentVariable("OAUTH_TOKEN", expectedValue);
            var configuration = new Configuration();

            var result = configuration.GetOAuthToken();

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetCloudIds_ReturnsCorrectValues()
        {
            Environment.SetEnvironmentVariable("CLOUD_ID_1", null);
            Environment.SetEnvironmentVariable("CLOUD_ID_2", null);
            
            var expectedValues = new HashSet<string> { "cloud-id-1", "cloud-id-2" };
            
            Environment.SetEnvironmentVariable("CLOUD_ID_1", "cloud-id-1");
            Environment.SetEnvironmentVariable("CLOUD_ID_2", "cloud-id-2");
            
            var configuration = new Configuration();
            
            var result = new HashSet<string>(configuration.GetCloudIds().Where(id => id != null)!);
            
            Assert.Equal(expectedValues, result);
        }

        [Fact]
        public void GetFolderIds_ReturnsCorrectValues()
        {
            Environment.SetEnvironmentVariable("FOLDER_ID_1", null);
            Environment.SetEnvironmentVariable("FOLDER_ID_2", null);
            
            var expectedValues = new HashSet<string> { "folder-id-1", "folder-id-2" };
            
            Environment.SetEnvironmentVariable("FOLDER_ID_1", "folder-id-1");
            Environment.SetEnvironmentVariable("FOLDER_ID_2", "folder-id-2");
            var configuration = new Configuration();

            var result = new HashSet<string>(configuration.GetFolderIds().Where(id => id != null)!);

            Assert.Equal(expectedValues, result);
        }

        [Fact]
        public void GetTimeZoneId_ReturnsCorrectValue()
        {
            Environment.SetEnvironmentVariable("TIME_ZONE_PROGRAM", null);
            
            var expectedValue = "Europe/Moscow";
            Environment.SetEnvironmentVariable("TIME_ZONE_PROGRAM", expectedValue);
            var configuration = new Configuration();

            var result = configuration.GetTimeZoneId();

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetTimeZoneId_ReturnsDefaultWhenNotSet()
        {
            var expectedValue = "UTC";
            Environment.SetEnvironmentVariable("TIME_ZONE_PROGRAM", null);
            var configuration = new Configuration();

            var result = configuration.GetTimeZoneId();
            
            Assert.Equal(expectedValue, result);
        }
}