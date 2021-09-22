using BotClient.Bussines.Interfaces;
using BotClient.Models.FileSystem.Enumerators;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {

        private readonly ISettingsService settingsService;
        private readonly IFileSystemService fileSystemService;

        public GoogleDriveService(ISettingsService SettingsService, IFileSystemService FileSystemService)
        {
            settingsService = SettingsService;
            fileSystemService = FileSystemService;
        }

        public async void GetFiles()
        {
            try
            {
                var credential = await Authorize();
                if (credential != null)
                {
                    var service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential
                    });

                    var request = service.Files.List();
                    var requestResult = await request.ExecuteAsync().ConfigureAwait(false);
                    var t = "";
                    for (int i = 0; i < requestResult.Files.Count; i++)
                        t += requestResult.Files[i].Name;
                    t = "";
                }
            }
            catch (Exception ex)
            {
                settingsService.AddLog("GoogleDriveService", ex);
            }
        }

        private async Task<UserCredential> Authorize()
        {
            UserCredential result = null;
            try
            {
                var credentialFilePath = fileSystemService.GetBasicFilePath(EnumBasicDirectoryType.Configuration, "credentials.json");
                if (credentialFilePath.isFileExist)
                {
                    await using (var credentialFileStream = new FileStream(credentialFilePath.FilePath, FileMode.Open, FileAccess.Read))
                    {
                        var tokenStorage = new FileDataStore("UserCredentialStoragePath", true);
                        result = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                 GoogleClientSecrets.FromStream(credentialFileStream).Secrets,
                                 new[] { DriveService.ScopeConstants.DriveReadonly },
                                 "GDriveService",
                                 CancellationToken.None,
                                 tokenStorage).Result;
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
                settingsService.AddLog("GoogleDriveService", ex);
            }
            return result;
        }
    }
}
