namespace FTPUpdaterAPI.Services.ConfigHandlingService
{
    internal interface IConfigHandlingService
    {
        internal string GetFtpAddress();
        internal bool CompareVersions(string latestVersionFileText);
        internal void UpdateLocalConfigVersion();
    }
}
