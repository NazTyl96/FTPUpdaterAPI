namespace FTPUpdaterAPI.Exceptions
{
    public class NoVersionException : Exception
    {
        public NoVersionException(bool isRemote)
            : base(isRemote ? "The configuration file on the FTP server doesn't have a version"
                            : "The configuration file on the local machine doesn't have a version") { }
    }
}
