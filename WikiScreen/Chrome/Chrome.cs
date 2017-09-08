using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using WikiScreen.Chrome.Requests;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WikiScreen.Chrome
{
    public class Chrome : IDisposable
    {
        private const string JsonPostfix = "/json";

        private const int BufferSize = 4096;

        private readonly string _remoteDebuggingUri;
        private Uri _sessionWsEndpoint;

        private ClientWebSocket _ws;

        private int _uniq_id;
        
        private Action<IChromeResponse> _onMessage = e => { };

        public Chrome(string remoteDebuggingUri)
        {
            _remoteDebuggingUri = remoteDebuggingUri;
        }

        public async Task Connect()
        {
            _ws = new ClientWebSocket();

            await _ws.ConnectAsync(_sessionWsEndpoint, CancellationToken.None);
        }

        public async void StartListen()
        {
            
            var buffer = new byte[BufferSize];

            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();


                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        
                        

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    var res = JsonConvert.DeserializeObject<ChromeResponse>(stringResult.ToString());

                    _onMessage(res);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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

        public Task<dynamic> PageEnable()
        {
            var cmd = new ChromeRequest
            {
                method = "Page.enable",
                @params = new Dictionary<string, dynamic>()
            };
            
            return SendCommand(cmd);
        }

        public Task<dynamic> Eval(string command, bool await_promise)
        {           
            var cmd = new ChromeRequest
            {
                method = "Runtime.evaluate",
                @params = new Dictionary<string, dynamic>
                {
                    {"expression", command},
                    //{"objectGroup", "console"},
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

            var messageBuffer = ToJsonBytes(cmd);
            var messagesCount = (int) Math.Ceiling((double) messageBuffer.Length / BufferSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = BufferSize * i;
                var count = BufferSize;
                var lastMessage = i + 1 == messagesCount;

                if (count * (i + 1) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }
                
                Console.WriteLine("send bytes");
                
                _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text,
                    lastMessage, CancellationToken.None);
            }
            
            Console.WriteLine("exec command " + current_id);
            
            return await Task.Run(() =>
            {
                var t = new TaskCompletionSource<dynamic>();

                void Cb(IChromeResponse s)
                {
                    if (s.id != current_id) return;
                    
                    Console.WriteLine("Awaiter for " + current_id, s.id, s.result);
                    
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

        private static byte[] ToJsonBytes<T>(T item)
        {
            var str = JsonConvert.SerializeObject(item, Formatting.None);
            
            Console.WriteLine(str);
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