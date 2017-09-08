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
                
                result = chrome.Eval(@"(function(selector) { return new Promise((fulfill, reject) => {
        const element = document.querySelector(selector);

        if(element) {
            fulfill();
            return;
        }

        new MutationObserver((mutations, observer) => {
            const nodes = [];
            
            mutations.forEach((mutation) => {
                nodes.push(...mutation.addedNodes);
            });
           
            if (nodes.find((node) => node.matches(selector))) {
                observer.disconnect();
                fulfill();
            }
        }).observe(document.body, {
            childList: true
        })
    }).then(() => {
        const element = document.querySelector(selector);

        const {left, top, width, height} = element.getBoundingClientRect();
        return {x: left, y: top, width, height};
    })
})("".item-box:first-child"")", true);

                result.Wait();

                var res = result.Result;
                Console.WriteLine(result.Result);
                
                Console.WriteLine(res["result"]["value"]);

                Console.ReadLine();
            }
        }
    }
}