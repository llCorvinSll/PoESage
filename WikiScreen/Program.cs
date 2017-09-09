using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WikiScreen.Chrome;

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
               
                //chrome.StartListen();
                
                Task<dynamic> result;
                
                result = chrome.PageEnable();
                result.Wait();
                
//                result = chrome.SetDeviceMetricsOverride(1600, 1600);
//                result.Wait();
//                
//                result = chrome.SetVisibleSize(1600, 1600);
//                result.Wait();


                var wait_for_page = chrome.WaitForPage();


                
               

                result = chrome.NavigateTo("https://pathofexile.gamepedia.com/Stone_Hammer");
                result.Wait();
                wait_for_page.Wait();
                
                result = chrome.GetLayoutMetrics();
                result.Wait();
                Console.WriteLine("Content size " + result.Result["contentSize"]);

                var content_width = Convert.ToInt32(result.Result["contentSize"]["width"].ToString());
                var content_height = Convert.ToInt32(result.Result["contentSize"]["height"].ToString());

                result = chrome.SetDeviceMetricsOverride(content_width, content_height);
                result.Wait();
//                
//                result = chrome.SetVisibleSize(1600, 1600);
//                result.Wait();

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

        var docRect = element.ownerDocument.documentElement.getBoundingClientRect();

        const {left, top, width, height, x, y} = element.getBoundingClientRect();
        return {x: left - docRect.left , y: top - docRect.top, width, height};
    })
})("".item-box:first-child"")", true);

                result.Wait();

                var res = result.Result;
                Console.WriteLine(result.Result);
                
                Console.WriteLine(res["result"]["value"]);

                var val = res["result"]["value"];
                result = chrome.ScreenElement(new Viewport
                {
                    x = val["x"],
                    y = val["y"],
                    width = val["width"],
                    height = val["height"],
                    scale = 1
                });

                result.Wait();
                
                //Console.WriteLine(result.Result);

                var raw_img = result.Result["data"];
                GetMessage(raw_img.ToString());
                

                Console.ReadLine();
            }
        }

        static void GetMessage(string decoded)
        {
            Console.WriteLine(decoded.Length);
            var bytes = Convert.FromBase64String(decoded);
            using (var imageFile = new FileStream("./image.png", FileMode.Create))
            {
                imageFile.Write(bytes ,0, bytes.Length);
                imageFile.Flush();
            }
        }
    }
}