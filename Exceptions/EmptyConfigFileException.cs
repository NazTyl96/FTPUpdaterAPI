namespace FTPUpdaterAPI.Exceptions
{
    public class EmptyConfigFileException : Exception
    {
        public EmptyConfigFileException(bool isRemote)
            : base(isRemote ? "Please, check the congiguration file on the FTP server"
                            : "Please, check the congiguration file on the the local machine") { }
    }
}
