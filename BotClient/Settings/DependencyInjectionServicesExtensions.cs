using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Services;
using BotFile.Bussines.Interfaces;
using BotFile.Bussines.Services;
using BotMySQL.Bussines.Interfaces;
using BotMySQL.Bussines.Interfaces.Composite;
using BotMySQL.Bussines.Interfaces.MySQL;
using BotMySQL.Bussines.Services;
using BotMySQL.Bussines.Services.Composite;
using BotMySQL.Bussines.Services.MySQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BotClient.Settings
{
    public static class DependencyInjectionServicesExtensions
    {
        public static IServiceCollection DependencyInjectionAll(this IServiceCollection services)
        {
            //bot client logic services
            services.AddSingleton<IWebDriverService, WebDriverService>();
            services.AddSingleton<IWebElementService, WebElementService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IBotWorkService, BotWorkService>();
            services.AddSingleton<IVkActionService, VkActionService>();
            services.AddSingleton<IMySQLService, MySQLService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITextService, TextService>();
            services.AddSingleton<IBotActionService, BotActionService>();
            services.AddSingleton<IFileService, FileService>();
            //data services
            //composite
            services.AddSingleton<IBotCompositeService, BotCompositeService>();
            services.AddSingleton<IClientCompositeService, ClientCompositeService>();
            services.AddSingleton<IMissionCompositeService, MissionCompositeService>();
            services.AddSingleton<IServerCompositeService, ServerCompositeService>();
            //single
            services.AddSingleton<IBotActionHistoryService, BotActionHistoryService>();
            services.AddSingleton<IBotClientRoleConnectorService, BotClientRoleConnectorService>();
            services.AddSingleton<IBotCustomizeService, BotCustomizeService>();
            services.AddSingleton<IBotMusicService, BotMusicService>();
            services.AddSingleton<IBotNewsService, BotNewsService>();
            services.AddSingleton<IBotPlatformGroupConnectorService, BotPlatformGroupConnectorService>();
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<IBotVideoService, BotVideoService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IDialogScreenshotService, DialogScreenshotService>();
            services.AddSingleton<IMessagesService, MessagesService>();
            services.AddSingleton<IMissionNodeService, MissionNodeService>();
            services.AddSingleton<IMissionService, MissionService>();
            services.AddSingleton<INodePatternService, NodePatternService>();
            services.AddSingleton<IPatternService, PatternService>();
            services.AddSingleton<IPhraseService, PhraseService>();
            services.AddSingleton<IPlatformGroupService, PlatformGroupService>();
            services.AddSingleton<IRoleMissionConnectorService, RoleMissionConnectorService>();
            services.AddSingleton<IRoleServerConnectorService, RoleServerConnectorService>();
            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<IVideoDictionaryService, VideoDictionaryService>();
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<IParsedClientService, ParsedClientService>();
            services.AddSingleton<IBotCustomizeSettingsService, BotCustomizeSettingsService>();
            services.AddSingleton<IBotNewsMissionConnectorService, BotNewsMissionConnectorService>();

            return services;
        }
    }
}
