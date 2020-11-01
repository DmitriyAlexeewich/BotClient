using System.ComponentModel;

namespace BotClient.Models.Enumerators
{
    public enum EnumWebDriverStatus
    {
        [Description("Браузер запущен")]
        Start = 0,
        [Description("Браузер готов к работе")]
        Ready = 1,
        [Description("Ошибка браузера")]
        Error = 2,
        [Description("Браузер закрыт")]
        Closed = 3,
        [Description("Браузер загружается")]
        Loading = 4,
        [Description("Браузер занят")]
        Blocked = 5
    }
}
