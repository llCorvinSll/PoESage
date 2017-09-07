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

namespace WikiScreen.Chrome
{
    public class Chrome : IDisposable
    {
        private const string JsonPostfix = "/json";

        private const int BufferSize = 1024;

        private readonly string _remoteDebuggingUri;
        private Uri _sessionWsEndpoint;

        private ClientWebSocket _ws;
        
        private Action<string> _onMessage;

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

                    Console.WriteLine("StartListen" + stringResult);

                    _onMessage(stringResult.ToString());
                }
            }
            catch (Exception)
            {
                
            }
            finally
            {
                _ws.Dispose();
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

        public Task<string> NavigateTo(string uri)
        {
            var json = @"{""method"":""Page.navigate"",""params"":{""url"":""" + uri + @"""},""id"":1}";
            return SendCommand(json,1);
        }

        public Task<string> WaitForReady()
        {
            var json = @"{""method"":""Page.enable"",""params"":{ },""id"":2}";
            return SendCommand(json,2);
        }

        public Task<string> Eval(string cmd)
        {
            var json = @"{""method"":""Runtime.evaluate"",""params"":{""expression"":""" + cmd +
                       @""",""objectGroup"":""console"",""includeCommandLineAPI"":true,""doNotPauseOnExceptions"":false,""returnByValue"":false},""id"":3}";
            return SendCommand(json,3);
        }

        private async Task<string> SendCommand(string cmd, int id)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            var messageBuffer = Encoding.UTF8.GetBytes(cmd);
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

                _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text,
                    lastMessage, CancellationToken.None);
            }


            
            return await Task.Run(() =>
            {
                var t = new TaskCompletionSource<string>();

                void Cb(string s)
                {
                    if (s.IndexOf(@"""id"":" + id) > -1)
                    {
                        t.TrySetResult(s);

                        _onMessage -= Cb;
                    }
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

        private T Deserialise<T>(Stream json)
        {
            var obj = Activator.CreateInstance<T>();
            var serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T) serializer.ReadObject(json);
            return obj;
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