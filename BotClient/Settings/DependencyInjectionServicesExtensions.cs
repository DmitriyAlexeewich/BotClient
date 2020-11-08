using BotClient.Bussines.Interfaces;
using BotClient.Bussines.Interfaces.Composite;
using BotClient.Bussines.Interfaces.MySQL;
using BotClient.Bussines.Services;
using BotClient.Bussines.Services.Composite;
using BotClient.Bussines.Services.MySQL;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Settings
{
    public static class DependencyInjectionServicesExtensions
    {
        public static IServiceCollection DependencyInjectionAll(this IServiceCollection services)
        {
            services.AddSingleton<IWebDriverService, WebDriverService>();
            services.AddSingleton<IWebElementService, WebElementService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IMySQLService, MySQLService>();
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IBotClientRoleConnectorService, BotClientRoleConnectorService>();
            services.AddSingleton<IBotCustomizeService, BotCustomizeService>();
            services.AddSingleton<IMessagesService, MessagesService>();
            services.AddSingleton<IMissionNodeService, MissionNodeService>();
            services.AddSingleton<IMissionService, MissionService>();
            services.AddSingleton<INodePatternService, NodePatternService>();
            services.AddSingleton<IPatternService, PatternService>();
            services.AddSingleton<IRoleMissionConnectorService, RoleMissionConnectorService>();
            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<IBotCompositeService, BotCompositeService>();
            services.AddSingleton<IClientCompositeService, ClientCompositeService>();
            services.AddSingleton<IMissionCompositeService, MissionCompositeService>();
            services.AddSingleton<IBotWorkService, BotWorkService>();
            services.AddSingleton<IVkActionService, VkActionService>();

            return services;
        }
    }
}
