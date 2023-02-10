using System.Xml.Linq;
using FTPUpdaterAPI.Utilities;
using FTPUpdaterAPI.Exceptions;

namespace FTPUpdaterAPI.Services.ConfigHandlingService
{
    internal class ConfigHandlingService: IConfigHandlingService
    {
        private string? _latestVersion; //the newest version of the main app
        private XDocument? _localConfig;

        public ConfigHandlingService()
        {
            _localConfig = XDocument.Load(DirectoriesUtility.GetFilePath("config.xml"));
        }

        string IConfigHandlingService.GetFtpAddress()
        {
            if (_localConfig?.Root != null)
                if (_localConfig.Root.Element("address") != null)
                    return _localConfig.Root.Element("address").Value;
                else
                    throw new NoFtpAddressException();
            else
                throw new EmptyConfigFileException(false);
        }

        private string GetLocalVersion()
        {
            if (_localConfig?.Root != null)
                if (_localConfig.Root.Element("version") != null)
                    return _localConfig.Root.Element("version").Value;
                else
                    throw new NoVersionException(false);
            else
                throw new EmptyConfigFileException(false);
        }

        bool IConfigHandlingService.CompareVersions(string latestVersionFileText)
        {
            string localVersion = GetLocalVersion();

            XDocument remoteConfig = XDocument.Parse(latestVersionFileText);
            if (remoteConfig.Root != null)
                if (remoteConfig.Root.Element("version") != null)
                    _latestVersion = remoteConfig.Root?.Element("version")?.Value;
                else
                    throw new NoVersionException(true);
            else
                throw new EmptyConfigFileException(true);


            DateTime localVersionDate = DateTime.Parse(localVersion);
            DateTime? latestVersionDate = null;
            if (_latestVersion != null)
                latestVersionDate = DateTime.Parse(_latestVersion);
            else
                throw new NoVersionException(true);

            // comparing the dates of versions
            return latestVersionDate > localVersionDate;

        }

        void IConfigHandlingService.UpdateLocalConfigVersion()
        {
            if (_localConfig?.Root != null)
                if (_localConfig.Root.Element("version") != null)
                {
                    _localConfig.Root?.Element("version")?.SetValue(_latestVersion ?? "");
                    _localConfig.Save(DirectoriesUtility.GetFilePath("config.xml"));
                }
                else
                    throw new NoVersionException(false);
            else
                throw new EmptyConfigFileException(false);
        }
    }
}
