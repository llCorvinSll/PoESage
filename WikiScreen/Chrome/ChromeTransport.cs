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

        public Action<JObject> onMessage = e =>
        {
            //Console.WriteLine("debug: " + e.result);
        };
        
        private static string ID_FIELD = @"id";
        private static string METHOD_FIELD = @"method";
        
        private WebSocket _ws;
        
        private const string JsonPostfix = "/json";

        private readonly string _remoteDebuggingUri;
        
        private Uri _sessionWsEndpoint;

        private int _uniq_id;
        
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
                where r.devtoolsFrontendUrl != null
                select r).ToList();
        }
        
        public void SetActiveSession(string sessionWSEndpoint)
        {
            _sessionWsEndpoint = new Uri(sessionWSEndpoint.Replace("ws://localhost", "ws://127.0.0.1"));
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

            _uniq_id++;

            var current_id = _uniq_id;

            cmd.id = current_id;

            var cmd_str = ToJsonString(cmd);

            _ws.Send(cmd_str);
            
            return await Task.Run(() =>
            {
                var t = new TaskCompletionSource<TRes>();

                void Cb(JObject s)
                {                   
                    if (GetId(s) != current_id) return;

                    var res = s.ToObject<TRes>();

                    t.TrySetResult(res);

                    onMessage -= Cb;
                }

                onMessage += Cb;

                return t.Task;
            });
        }

        public static int GetId(JObject obj)
        {
            if (obj[ID_FIELD] == null) return -1;
            var id = obj[ID_FIELD].Value<int>();

            return id;
        }
        
        public static string GetMethod(JObject obj)
        {
            if (obj[METHOD_FIELD] == null) return "";
            var method = obj[METHOD_FIELD].Value<string>();

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

            onMessage(res); 
        }

        private void _OnWS_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }
        
        private void _OnWS_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("CLOSE CLOSE CLOSE");
        }

        private void _OnWS_Open(object sender, EventArgs e)
        {
            _opened.Set();
        }
        
        private void _OnWS_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Raw Data ");
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
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T) serializer.ReadObject(ms);
                return obj;
            }
        }

        #endregion
    }
}