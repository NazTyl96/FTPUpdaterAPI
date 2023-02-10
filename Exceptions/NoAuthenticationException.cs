namespace FTPUpdaterAPI.Exceptions
{
    internal class NoAuthenticationException : Exception
    {
        public NoAuthenticationException()
            : base(@"Your session has expired. Please log in") { }
    }
}
