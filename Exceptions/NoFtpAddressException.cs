namespace FTPUpdaterAPI.Exceptions
{
    public class NoFtpAddressException : Exception
    {
        public NoFtpAddressException()
            : base("The configuration file doesn't have an address of the FTP server") { }
    }
}
