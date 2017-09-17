using System;
using Newtonsoft.Json;

namespace WikiScreen.Chrome.Requests
{
    public interface IChromeResponse
    {
        string Method { get; set; }
        int? Id { get; set; }  
    }
    
    [Serializable]
    public class ChromeResponse<TRes> : IChromeResponse
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
        
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }
        
        [JsonProperty(PropertyName = "result")]
        public TRes Result { get; set; }
    }
}