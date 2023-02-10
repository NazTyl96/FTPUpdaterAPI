namespace FTPUpdaterAPI.Services.FTPHandlingService
{
    internal interface IFTPHandlingService
    {
        internal Task<string> GetVersionFile(string? ftpAddress);
        internal Task GetFilesForUpdate();
    }
}
