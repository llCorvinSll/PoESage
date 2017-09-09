using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using WikiScreen.Chrome.Requests;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocket4Net;

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
        private const string JsonPostfix = "/json";

        private const int BufferSize = 1024 * 1024 * 1024;//4096 * 32;

        private readonly string _remoteDebuggingUri;
        private Uri _sessionWsEndpoint;

        private WebSocket _ws;

        private int _uniq_id;
        
        private ManualResetEvent _opened = new ManualResetEvent(false);
        
        private Action<IChromeResponse> _onMessage = e =>
        {
            //Console.WriteLine("debug: " + e.result);
        };

        public Chrome(string remoteDebuggingUri)
        {
            _remoteDebuggingUri = remoteDebuggingUri;
        }

        public async Task Connect()
        {
            _opened.Reset();
            
            _ws = new WebSocket(_sessionWsEndpoint.ToString());

            _ws.Opened += _OnWS_Open;
            _ws.MessageReceived += _OnWS_MessageReceived;
            _ws.DataReceived += _OnWS_DataReceived;
            _ws.Error += _OnWS_Error;
            _ws.Closed += _OnWS_Closed;

            
            _ws.Open();
            await Task.Run(() =>
            {
                _opened.WaitOne();
            });
        }

        
        
        private void _OnWS_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }
        
        private void _OnWS_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("CLOSE  CLOSE CLOSE ");
        }

        private void _OnWS_Open(object sender, EventArgs e)
        {
            _opened.Set();
        }

        private void _OnWS_MessageReceived(object sender, MessageReceivedEventArgs  e)
        {
            var stringResult = new StringBuilder();
            
            stringResult.Append(e.Message);
            
            Console.WriteLine("Raw message ");
            
            var res = JsonConvert.DeserializeObject<ChromeResponse>(stringResult.ToString());

            _onMessage(res); 
        }

        private void _OnWS_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Raw Data ");
        }

        private TRes SendRequest<TRes>()
        {
            var req = (HttpWebRequest) WebRequest.Create(_remoteDebuggingUri + JsonPostfix);
            var resp = req.GetResponse();
            var respStream = resp.GetResponseStream();

            var sr = new StreamReader(respStream);
            var s = sr.ReadToEnd();
            resp.Dispose();
            return Deserialise<TRes>(s);
        }

        public List<RemoteSessionsResponse> GetAvailableSessions()
        {
            var res = SendRequest<List<RemoteSessionsResponse>>();
            return (from r in res
                where r.devtoolsFrontendUrl != null
                select r).ToList();
        }

        public Task<dynamic> NavigateTo(string uri)
        {
            var cmd = new ChromeRequest
            {
                method = "Page.navigate",
                @params = new Dictionary<string, dynamic> {
                    { "url", uri }
                }
            };
            
            return SendCommand(cmd);
        }
        
        //
        
        public Task<dynamic> GetLayoutMetrics()
        {
            var cmd = new ChromeRequest
            {
                method = "Page.getLayoutMetrics",
                @params = new Dictionary<string, dynamic>()
            };
            
            return SendCommand(cmd);
        }

        public Task<dynamic> PageEnable()
        {
            var cmd = new ChromeRequest
            {
                method = "Page.enable",
                @params = new Dictionary<string, dynamic>()
            };

            return SendCommand(cmd);
        }

        public Task<dynamic> ScreenElement(Viewport viewport)
        {
            var cmd = new ChromeRequest
            {
                method = "Page.captureScreenshot",
                @params = new Dictionary<string, dynamic> {
                    { "clip", viewport },
                    { "format", "png" },
                    //{ "quality", 0}
                }
            };
            
            return SendCommand(cmd);
            
        }

        public Task<dynamic> SetDeviceMetricsOverride(int w, int h) 
        {
            var cmd = new ChromeRequest
            {
                method = "Emulation.setDeviceMetricsOverride",
                @params = new Dictionary<string, dynamic>
                {
                    {"width", w },
                    {"screenWidth", w },
                    {"height", h },
                    {"screenHeight", h },
                    {"positionX", 0 },
                    {"positionY", 0 },
                    {"deviceScaleFactor", 1 },
                    {"mobile", false },
                    {"fitWindow", true }
                }
            };
            return SendCommand(cmd);
            
        }

        public Task<dynamic> SetVisibleSize(int w, int h) 
        {
            var cmd = new ChromeRequest
            {
                method = "Emulation.setVisibleSize",
                @params = new Dictionary<string, dynamic>
                {
                    {"width", w },
                    {"height", h }
                }
            };
            return SendCommand(cmd);
            
        }
        //setVisibleSize

        public async Task<dynamic> WaitForPage()
        {
            return await Task.Run(() =>
            {
                var t = new TaskCompletionSource<dynamic>();

                void Cb(IChromeResponse s)
                {
                    if (s.method != "Page.loadEventFired") return;

                    t.TrySetResult(s.result);

                    _onMessage -= Cb;
                }

                _onMessage += Cb;

                return t.Task;
            });
        }
        
        public Task<dynamic> Eval(string command, bool await_promise)
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
            return SendCommand(cmd);
        }

        private async Task<dynamic> SendCommand(IChromeRequest cmd)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            _uniq_id++;

            var current_id = _uniq_id;

            cmd.id = current_id;

            var cmd_str = ToJsonString(cmd);
            
            Console.WriteLine(cmd_str);
            _ws.Send(cmd_str);
            
            Console.WriteLine("exec command " + current_id);
            
            return await Task.Run(() =>
            {
                var t = new TaskCompletionSource<dynamic>();

                void Cb(IChromeResponse s)
                {
                    if (s.id != current_id) return;

                    t.TrySetResult(s.result);

                    _onMessage -= Cb;
                }

                _onMessage += Cb;

                return t.Task;
            });
        }

        private static T Deserialise<T>(string json)
        {
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T) serializer.ReadObject(ms);
                return obj;
            }
        }


        private static string ToJsonString<T>(T item)
        {
            var str = JsonConvert.SerializeObject(item, Formatting.None);

            return str;
        }

        private static byte[] ToJsonBytes<T>(T item)
        {
            var str = ToJsonString(item);

            return Encoding.UTF8.GetBytes(str);
        }
    
        public void SetActiveSession(string sessionWSEndpoint)
        {
            _sessionWsEndpoint = new Uri(sessionWSEndpoint.Replace("ws://localhost", "ws://127.0.0.1"));
        }

        public void Dispose()
        {
            _ws.Dispose();
        }
    }
}