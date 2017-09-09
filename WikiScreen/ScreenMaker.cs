using System;
using System.IO;
using System.Threading.Tasks;
using WikiScreen.Chrome;
using WikiScreen.Chrome.Requests.Response;

namespace WikiScreen
{
    public class ScreenMaker
    {
        public static async Task<byte[]> MakeWikiScreen(string remote_chrome, string url, double scale_factor)
        {
            using (var chrome = new Chrome.Chrome(remote_chrome))
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

                await chrome.Connect();
                
                await chrome.PageEnable();
                
                var wait_for_page = chrome.WaitForPage();
                
                await chrome.NavigateTo(url);

                await wait_for_page;
                
                
                var layout_metrics = await chrome.GetLayoutMetrics();

                Console.WriteLine("Content size " + layout_metrics.result.contentSize);

                var content_width = Convert.ToInt32(layout_metrics.result.contentSize.width);
                var content_height = Convert.ToInt32(layout_metrics.result.contentSize.height);

                await chrome.SetDeviceMetricsOverride(content_width, content_height, scale_factor);
                
                
                var image_size = await chrome.Eval<ElementBoundReactResultResponse>(@"(function(selector) { return new Promise((fulfill, reject) => {
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

                var val = image_size.result.result.value;
                
                var screenshot = await chrome.ScreenElement(new Viewport
                {
                    x = val.x * scale_factor,
                    y = val.y * scale_factor,
                    width = val.width ,
                    height = val.height ,
                    scale = 1
                });
                
                var raw_img = screenshot.result["data"];
                
                GetMessage(raw_img.ToString());

                return raw_img;

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