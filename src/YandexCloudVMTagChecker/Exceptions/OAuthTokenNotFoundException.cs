namespace YandexCloudVMTagChecker.Exceptions;

public class OAuthTokenNotFoundException : Exception
{
    public OAuthTokenNotFoundException()
        : base("OAuth token cannot be null.") { }
}