namespace FTPUpdaterAPI.Services.IISHandlingService
{
    internal interface IIISHandlingService
    {
        internal void StopAppPools();
        internal void StartAppPools();
    }
}
