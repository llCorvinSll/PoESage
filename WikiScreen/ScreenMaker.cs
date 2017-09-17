using System;
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

                if (sessions.Count == 0)
                    throw new Exception("All debugging sessions are taken.");

                // Will drive first tab session
                var sessionWSEndpoint =
                    sessions[0].WebSocketDebuggerUrl;

                chrome.SetActiveSession(sessionWSEndpoint);

                await chrome.Connect();
                
                await chrome.PageEnable();
                
                var wait_for_page = chrome.WaitForPage();
                
                await chrome.NavigateTo(url);

                await wait_for_page;
                
                
                var layout_metrics = await chrome.GetLayoutMetrics();
                var content_width = Convert.ToInt32(layout_metrics.Result.ContentSize.Width);
                var content_height = Convert.ToInt32(layout_metrics.Result.ContentSize.Height);

                await chrome.SetDeviceMetricsOverride(content_width, content_height, scale_factor);

                var image_size = await chrome.GetBoundingRectBySelector(".item-box:first-child");

                var val = image_size.Result.Result.Value;
                
                var screenshot = await chrome.ScreenElement(new Viewport
                {
                    X = val.X.Value * scale_factor,
                    Y = val.Y.Value * scale_factor,
                    Width = val.Width.Value ,
                    Height = val.Height.Value ,
                    Scale = 1
                });
                
                var raw_img = screenshot.Result.Data;

                return raw_img;
            }
        }
    }
}