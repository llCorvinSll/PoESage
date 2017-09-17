using System;
using System.Collections.Generic;
using WikiScreen.Chrome.Requests;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WikiScreen.Chrome.Requests.Response;

namespace WikiScreen.Chrome
{
    public class Viewport
    {
        [JsonProperty(PropertyName = "x")]
        public double X { get; set; }
        
        [JsonProperty(PropertyName = "y")]
        public double Y { get; set; }
        
        [JsonProperty(PropertyName = "width")]
        public double Width { get; set; }
        
        [JsonProperty(PropertyName = "height")]
        public double Height { get; set; }
        
        [JsonProperty(PropertyName = "scale")]
        public double Scale { get; set; }
    }

    public class Chrome : IDisposable
    {
        private readonly ChromeTransport _tr;

        public Chrome(string remoteDebuggingUri)
        {
            _tr = new ChromeTransport(remoteDebuggingUri);
        }

        public async Task Connect()
        {
            await _tr.Connect();
        }

        public List<RemoteSessionsResponse> GetAvailableSessions()
        {
            return _tr.GetAvailableSessions();
        }

        public Task<ChromeResponse<object>> NavigateTo(string uri)
        {
            var cmd = new ChromeRequest
            {
                Method = "Page.navigate",
                Params = new Dictionary<string, dynamic>
                {
                    {"url", uri}
                }
            };

            return _tr.SendCommand<ChromeResponse<object>>(cmd);
        }

        //

        public Task<GetLayoutMetricsResponse> GetLayoutMetrics()
        {
            var cmd = new ChromeRequest
            {
                Method = "Page.getLayoutMetrics",
                Params = new Dictionary<string, dynamic>()
            };

            return _tr.SendCommand<GetLayoutMetricsResponse>(cmd);
        }

        public Task<ChromeResponse<dynamic>> PageEnable()
        {
            var cmd = new ChromeRequest
            {
                Method = "Page.enable",
                Params = new Dictionary<string, dynamic>()
            };

            return _tr.SendCommand<ChromeResponse<dynamic>>(cmd);
        }

        public Task<CaptureScreenshotResponse> ScreenElement(Viewport viewport)
        {
            var cmd = new ChromeRequest
            {
                Method = "Page.captureScreenshot",
                Params = new Dictionary<string, dynamic>
                {
                    {"clip", viewport},
                    {"format", "png"}
                }
            };

            return _tr.SendCommand<CaptureScreenshotResponse>(cmd);
        }

        public Task<ChromeResponse<dynamic>> SetDeviceMetricsOverride(int w, int h, double scaleFactor)
        {
            var cmd = new ChromeRequest
            {
                Method = "Emulation.setDeviceMetricsOverride",
                Params = new Dictionary<string, dynamic>
                {
                    {"width", w},
                    {"screenWidth", w},
                    {"height", h},
                    {"screenHeight", h},
                    {"positionX", 0},
                    {"positionY", 0},
                    {"deviceScaleFactor", scaleFactor},
                    {"mobile", false},
                    {"fitWindow", true}
                }
            };
            return _tr.SendCommand<ChromeResponse<dynamic>>(cmd);
        }

        public Task<ElementBoundReactResultResponse> GetBoundingRectBySelector(string selector)
        {
            return Eval<ElementBoundReactResultResponse>(ChromeJsCommands.GetElementBoundsAsync(selector), true);
        }

        public async Task<dynamic> WaitForPage()
        {
            return await Task.Run(() =>
            {
                var t = new TaskCompletionSource<dynamic>();

                void Cb(JObject s)
                {
                    if (ChromeTransport.GetMethod(s) != "Page.loadEventFired") return;

                    t.TrySetResult(s);

                    if (_tr.OnMessage != null) _tr.OnMessage -= Cb;
                }

                _tr.OnMessage += Cb;

                return t.Task;
            });
        }

        private Task<TRes> Eval<TRes>(string command, bool awaitPromise) where TRes : IChromeResponse
        {
            var cmd = new ChromeRequest
            {
                Method = "Runtime.evaluate",
                Params = new Dictionary<string, dynamic>
                {
                    {"expression", command},
                    {"includeCommandLineAPI", true},
                    {"doNotPauseOnExceptions", false},
                    {"returnByValue", true},
                    {"awaitPromise", awaitPromise}
                }
            };
            return _tr.SendCommand<TRes>(cmd);
        }

        public void SetActiveSession(string sessionWsEndpoint)
        {
            _tr.SetActiveSession(sessionWsEndpoint);
        }

        public void Dispose()
        {
            _tr.Dispose();
        }
    }
}