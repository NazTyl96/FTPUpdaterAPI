using FTPUpdaterAPI;
using FTPUpdaterAPI.Services.AuthorizationService;
using FTPUpdaterAPI.Services.ConfigHandlingService;
using FTPUpdaterAPI.Services.FTPHandlingService;
using FTPUpdaterAPI.Services.IISHandlingService;
using FTPUpdaterAPI.Services.FilesReplacementService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<UpdateFacade>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IConfigHandlingService, ConfigHandlingService>();
builder.Services.AddScoped<IFTPHandlingService, FTPHandlingService>();
builder.Services.AddScoped<IIISHandlingService, IISHandlingService>();
builder.Services.AddScoped<IFilesReplacementService, FilesReplacementService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/update", async (HttpContext context, UpdateFacade updater) =>
{
    return await updater.Update(context);
});

app.Run();