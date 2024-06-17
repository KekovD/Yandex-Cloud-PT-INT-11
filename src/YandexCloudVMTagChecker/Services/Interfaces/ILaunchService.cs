namespace YandexCloudVMTagChecker.Services.Interfaces;

public interface ILaunchService
{
    Task Launch(IList<string?> cloudIdList, IList<string?> folderIdList);
}