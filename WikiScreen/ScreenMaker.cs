using System;
using System.IO;
using System.Threading.Tasks;
using WikiScreen.Chrome;

namespace WikiScreen
{
    public class ScreenMaker
    {
        public static async Task<string> MakeWikiScreen(string remote_chrome, string url, double scale_factor)
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
                var content_width = Convert.ToInt32(layout_metrics.result.contentSize.width);
                var content_height = Convert.ToInt32(layout_metrics.result.contentSize.height);

                await chrome.SetDeviceMetricsOverride(content_width, content_height, scale_factor);

                var image_size = await chrome.getBoundingRectBySelector(".item-box:first-child");

                var val = image_size.result.result.value;
                
                var screenshot = await chrome.ScreenElement(new Viewport
                {
                    x = val.x * scale_factor,
                    y = val.y * scale_factor,
                    width = val.width ,
                    height = val.height ,
                    scale = 1
                });
                
                var raw_img = screenshot.result.data;
                
                GetMessage(raw_img);

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