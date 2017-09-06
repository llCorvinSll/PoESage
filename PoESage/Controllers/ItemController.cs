using Microsoft.AspNetCore.Mvc;
using PoESage.Services;
using Telegram.Bot.Types;

namespace PoESage.Controllers
{
    [Route("api/[controller]")]
    public class ItemController : Controller
    {
        readonly ISageService _sageService;
        readonly BotConfiguration _config;

        public ItemController(ISageService sageService, BotConfiguration config)
        {
            _sageService = sageService;
            _config = config;
        }

        // POST api/update
        [HttpPost]
        public void Post([FromBody] Update update)
        {
            _sageService.Echo(update);
        }
    }
}