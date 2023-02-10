namespace FTPUpdaterAPI.Services.FilesReplacementService
{
    internal interface IFilesReplacementService
    {
        internal void ReplaceAllProjectFiles();
        internal void ClearTempDirectory();
    }
}
