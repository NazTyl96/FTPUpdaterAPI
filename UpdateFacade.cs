using FTPUpdaterAPI.Services.AuthorizationService;
using FTPUpdaterAPI.Services.ConfigHandlingService;
using FTPUpdaterAPI.Services.FTPHandlingService;
using FTPUpdaterAPI.Services.IISHandlingService;
using FTPUpdaterAPI.Services.FilesReplacementService;
using FTPUpdaterAPI.Exceptions;
using System.Net;


namespace FTPUpdaterAPI
{
    internal class UpdateFacade
    {
        private IAuthorizationService _authorizationService;
        private IConfigHandlingService _configService;
        private IFTPHandlingService _ftpService;
        private IIISHandlingService _iisService;
        private IFilesReplacementService _filesService;

        public UpdateFacade(IAuthorizationService authorizationService,
                            IConfigHandlingService configService,
                            IFTPHandlingService ftpService,
                            IIISHandlingService iisService,
                            IFilesReplacementService filesService)
        {
            _authorizationService = authorizationService;
            _configService = configService;
            _ftpService = ftpService;
            _iisService = iisService;
            _filesService = filesService;
        }

        internal async Task<IResult> Update(HttpContext context)
        {
            try
            {
                // checking if user has the the rights of admin/support
                bool isAdmin = await _authorizationService
                                     .CheckIfUserIsAdmin(context.Request.Cookies["NameOfYourTicket"]);
                if (!isAdmin)
                {
                    // 403 code - forbidden
                    return Results.Forbid();
                }

                // receiving xml file with the latest version from the FTP server
                string versionFileText = await _ftpService.GetVersionFile(_configService.GetFtpAddress());

                // comparing the local version with the latest from FTP server
                bool needsUpdate = _configService.CompareVersions(versionFileText);
                if (!needsUpdate)
                {
                    // 204 code with a notification about local version being the latest
                    return Results.Json(new { message = "Your application is up to date" },
                                        null, "application/json", 204);
                }

                // downloading the update
                await _ftpService.GetFilesForUpdate();

                // stopping the application pools
                _iisService.StopAppPools();

                // replacing files
                _filesService.ReplaceAllProjectFiles();

                _configService.UpdateLocalConfigVersion();

                // 200 code, everything's ok
                return Results.Ok();
            }
            catch (NoAuthenticationException ex)
            {
                // 401 code and notification that user nedd to log in again
                return Results.Json(new { message = ex.Message }, null, "application/json", 401);
            }
            catch (WebException ex)
            {
                // notification that FTP server is unavailable (code 408 - request timeout)
                return Results.Json(new { message = ex.Message }, null, "application/json", 408);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                // notification that some of the pools were not started and need to be started manually
                return Results.Json(new { message = ex.Message }, null, "application/json", 503);
            }
            catch (Exception ex)
            {
                // notification "something went wrong" and a stacktrace
                return Results.Json(new { exceptionMessage = $"{ex.Message} in method {ex.TargetSite}. Stacktrace: {ex.StackTrace}" }, 
                                    null, "application/json", 500);
            }
            finally
            {
                // in any outcome starting the pools and cleaning the temporary directory
                _iisService.StartAppPools();
                _filesService.ClearTempDirectory();
            }
        }
    }
}
