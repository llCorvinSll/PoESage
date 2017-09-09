using System;
using System.Collections.Generic;
using WikiScreen.Chrome.Requests;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WikiScreen.Chrome.Requests.Response;

namespace WikiScreen.Chrome
{
    public class Viewport
    {
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public double scale { get; set; }
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
                method = "Page.navigate",
                @params = new Dictionary<string, dynamic>
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
                method = "Page.getLayoutMetrics",
                @params = new Dictionary<string, dynamic>()
            };

            return _tr.SendCommand<GetLayoutMetricsResponse>(cmd);
        }

        public Task<ChromeResponse<dynamic>> PageEnable()
        {
            var cmd = new ChromeRequest
            {
                method = "Page.enable",
                @params = new Dictionary<string, dynamic>()
            };

            return _tr.SendCommand<ChromeResponse<dynamic>>(cmd);
        }

        public Task<CaptureScreenshotResponse> ScreenElement(Viewport viewport)
        {
            var cmd = new ChromeRequest
            {
                method = "Page.captureScreenshot",
                @params = new Dictionary<string, dynamic>
                {
                    {"clip", viewport},
                    {"format", "png"}
                }
            };

            return _tr.SendCommand<CaptureScreenshotResponse>(cmd);
        }

        public Task<ChromeResponse<dynamic>> SetDeviceMetricsOverride(int w, int h, double scale_factor)
        {
            var cmd = new ChromeRequest
            {
                method = "Emulation.setDeviceMetricsOverride",
                @params = new Dictionary<string, dynamic>
                {
                    {"width", w},
                    {"screenWidth", w},
                    {"height", h},
                    {"screenHeight", h},
                    {"positionX", 0},
                    {"positionY", 0},
                    {"deviceScaleFactor", scale_factor},
                    {"mobile", false},
                    {"fitWindow", true}
                }
            };
            return _tr.SendCommand<ChromeResponse<dynamic>>(cmd);
        }

        public Task<ElementBoundReactResultResponse> getBoundingRectBySelector(string selector)
        {
            return Eval<ElementBoundReactResultResponse>(@"
(function(selector) { return new Promise((fulfill, reject) => {
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
})('" + selector + "')", true);
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

                    _tr.onMessage -= Cb;
                }

                _tr.onMessage += Cb;

                return t.Task;
            });
        }

        public Task<TRes> Eval<TRes>(string command, bool await_promise) where TRes : IChromeResponse
        {
            var cmd = new ChromeRequest
            {
                method = "Runtime.evaluate",
                @params = new Dictionary<string, dynamic>
                {
                    {"expression", command},
                    {"includeCommandLineAPI", true},
                    {"doNotPauseOnExceptions", false},
                    {"returnByValue", true},
                    {"awaitPromise", await_promise}
                }
            };
            return _tr.SendCommand<TRes>(cmd);
        }

        public void SetActiveSession(string sessionWSEndpoint)
        {
            _tr.SetActiveSession(sessionWSEndpoint);
        }

        public void Dispose()
        {
            _tr.Dispose();
        }
    }
}