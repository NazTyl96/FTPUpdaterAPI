namespace FTPUpdaterAPI.Services.AuthorizationService
{
    internal interface IAuthorizationService
    {
        internal Task<bool> CheckIfUserIsAdmin(string? ticket);
    }
}
