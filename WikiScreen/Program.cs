using System;

namespace WikiScreen
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var chrome = new Chrome.Chrome("http://localhost:9222"))
            {
                var sessions = chrome.GetAvailableSessions();

                Console.WriteLine("Available debugging sessions");
                foreach (var s in sessions)
                    Console.WriteLine(s.url);

                if (sessions.Count == 0)
                    throw new Exception("All debugging sessions are taken.");

                // Will drive first tab session
                var sessionWSEndpoint =
                    sessions[0].webSocketDebuggerUrl;

                chrome.SetActiveSession(sessionWSEndpoint);
                var connection = chrome.Connect();
                connection.Wait();

                chrome.StartListen();

                var result = chrome.NavigateTo("https://pathofexile.gamepedia.com/Star_of_Wraeclast");

                result.Wait();

                result = chrome.WaitForReady();

                result.Wait();

                Console.WriteLine(result.Result);

                //result = chrome.Eval("document.getElementsByClassName('item-box')[0].style.backgroundColor = 'red'");
                result = chrome.Eval(@"(() => {
                const element = document.querySelector('.item-box:first-child');
                if (!element)
                    return 'asdsd';
                const {x, y, width, height} = element.getBoundingClientRect();
                return {left: x, top: y, width, height, id: element.id};
               })()");

                result.Wait();

                Console.WriteLine(result.Result);

                Console.ReadLine();
            }
        }
    }
}