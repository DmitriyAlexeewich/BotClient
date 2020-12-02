using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Services;
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
            //data services
            services.AddSingleton<IBotCompositeService, BotCompositeService>();
            services.AddSingleton<IClientCompositeService, ClientCompositeService>();
            services.AddSingleton<IMissionCompositeService, MissionCompositeService>();
            services.AddSingleton<IBotClientRoleConnectorService, BotClientRoleConnectorService>();
            services.AddSingleton<IBotCustomizeService, BotCustomizeService>();
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IMessagesService, MessagesService>();
            services.AddSingleton<IMissionService, MissionService>();
            services.AddSingleton<IMissionNodeService, MissionNodeService>();
            services.AddSingleton<INodePatternService, NodePatternService>();
            services.AddSingleton<IPatternService, PatternService>();
            services.AddSingleton<IRoleMissionConnectorService, RoleMissionConnectorService>();
            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<IDialogScreenshotService, DialogScreenshotService>();

            return services;
        }
    }
}
