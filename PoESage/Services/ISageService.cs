using Telegram.Bot.Types;

namespace PoESage.Services
{
    public interface ISageService
    {
        void Echo(Update update);
    }
}