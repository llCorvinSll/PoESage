using System.Collections.Generic;
using Telegram.Bot;

namespace PoESage.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
        List<int> MailReceiver { get; }
    }
}