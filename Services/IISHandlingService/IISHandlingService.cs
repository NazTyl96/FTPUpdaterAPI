using Microsoft.Web.Administration;
using FTPUpdaterAPI.Utilities;

namespace FTPUpdaterAPI.Services.IISHandlingService
{
    internal class IISHandlingService: IIISHandlingService
    {
        private ServerManager _serverManager;
        private readonly string _baseName;

        public IISHandlingService()
        {
            _serverManager = new ServerManager();
            _baseName = DirectoriesUtility.GetBaseName();
        }

        void IIISHandlingService.StopAppPools()
        {

            #region your IIS pools
            ApplicationPool? appPool = _serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.Equals("YourPoolName"));

            appPool?.Stop();
            #endregion
        }

        void IIISHandlingService.StartAppPools()
        {

            # region your IIS pools
            ApplicationPool? appPool = _serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.Equals("YourPoolName"));

            bool hasStarted = false;
            while (hasStarted != true)
            {
                try
                {
                    appPool?.Start();

                    hasStarted = true;
                }
                catch (Exception)
                {
                    // catching the exception about service being unavailable and try to start it again
                }
            }
            #endregion
        }
    }
}
