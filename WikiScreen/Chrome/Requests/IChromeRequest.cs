using System.Collections.Generic;
using Newtonsoft.Json;

namespace WikiScreen.Chrome.Requests
{
    public interface IChromeRequest
    {
        int Id { get; set; } 
        string Method { get; set; }
        Dictionary<string, dynamic> Params { get; set; }
    }


    public class ChromeRequest : IChromeRequest
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
        
        [JsonProperty(PropertyName = "params")]
        public Dictionary<string, dynamic> Params { get; set; }
    }
}
