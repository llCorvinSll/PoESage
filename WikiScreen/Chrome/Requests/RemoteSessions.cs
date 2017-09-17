using Newtonsoft.Json;

namespace WikiScreen.Chrome.Requests
{
    public class RemoteSessionsResponse
    {
        [JsonProperty(PropertyName = "devtoolsFrontendUrl")]
        public string DevtoolsFrontendUrl;

        [JsonProperty(PropertyName = "faviconUrl")]
        public string FaviconUrl;

        [JsonProperty(PropertyName = "thumbnailUrl")]
        public string ThumbnailUrl;

        [JsonProperty(PropertyName = "title")]
        public string Title;
        
        [JsonProperty(PropertyName = "url")]
        public string Url;

        [JsonProperty(PropertyName = "webSocketDebuggerUrl")]
        public string WebSocketDebuggerUrl;
    }
}