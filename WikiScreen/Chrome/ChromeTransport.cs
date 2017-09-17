using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;
using WikiScreen.Chrome.Requests;

namespace WikiScreen.Chrome
{
    public sealed class ChromeTransport : IDisposable
    {
        public ChromeTransport(string remoteDebuggingUri)
        {
            _remoteDebuggingUri = remoteDebuggingUri;
        }

        public Action<JObject> OnMessage = e =>
        {
        };

        private const string IdField = @"id";
        private const string MethodField = @"method";

        private WebSocket _ws;
        
        private const string JsonPostfix = "/json";

        private readonly string _remoteDebuggingUri;
        
        private Uri _sessionWsEndpoint;

        private int _uniqId;
        
        private readonly ManualResetEvent _opened = new ManualResetEvent(false);

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
                where r.DevtoolsFrontendUrl != null
                select r).ToList();
        }
        
        public void SetActiveSession(string sessionWsEndpoint)
        {
            _sessionWsEndpoint = new Uri(sessionWsEndpoint.Replace("ws://localhost", "ws://127.0.0.1"));
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
        
        public async Task<TRes> SendCommand<TRes>(IChromeRequest cmd) where TRes : IChromeResponse
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            _uniqId++;

            var currentId = _uniqId;

            cmd.Id = currentId;

            var cmdStr = ToJsonString(cmd) ?? throw new ArgumentNullException(nameof(cmd));

            _ws.Send(cmdStr);
            
            return await Task.Run(() =>
            {
                var t = new TaskCompletionSource<TRes>();

                void Cb(JObject s)
                {                   
                    if (GetId(s) != currentId) return;

                    var res = s.ToObject<TRes>();

                    t.TrySetResult(res);

                    if (OnMessage != null) OnMessage -= Cb;
                }

                OnMessage += Cb;

                return t.Task;
            });
        }

        public static int? GetId(JObject obj)
        {
            var id = obj[IdField]?.Value<int>();

            return id;
        }
        
        public static string GetMethod(JObject obj)
        {
            var method = obj[MethodField]?.Value<string>();

            return method;
        }
        
        public void Dispose()
        {
            _ws.Close();
        }

        #region ws_listners
        
        private void _OnWS_MessageReceived(object sender, MessageReceivedEventArgs  e)
        {
            var stringResult = new StringBuilder();
            
            stringResult.Append(e.Message);

            var res = JObject.Parse(stringResult.ToString());

            OnMessage(res); 
        }

        private void _OnWS_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
        }
        
        private void _OnWS_Closed(object sender, EventArgs e)
        {
        }

        private void _OnWS_Open(object sender, EventArgs e)
        {
            _opened.Set();
        }
        
        private void _OnWS_DataReceived(object sender, DataReceivedEventArgs e)
        {
        }
        
        #endregion

        #region converters

        private static string ToJsonString<T>(T item)
        {
            var str = JsonConvert.SerializeObject(item, Formatting.None);

            return str;
        }
        
        private static T Deserialise<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        #endregion
    }
}