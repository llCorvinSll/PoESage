using Newtonsoft.Json;

namespace WikiScreen.Chrome.Requests.Response
{
    public class ElementBoundRect
    {
        [JsonProperty(PropertyName = "x")]
        public double? X { get; set; }
        
        [JsonProperty(PropertyName = "y")]
        public double? Y { get; set; }
        
        [JsonProperty(PropertyName = "width")]
        public double? Width { get; set; }
        
        [JsonProperty(PropertyName = "height")]
        public double? Height { get; set; }
    }
    
    public class ElementBoundRectResult 
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        
        [JsonProperty(PropertyName = "value")]
        public ElementBoundRect Value { get; set; }
    }

    public class ScriptExecResult<TRes>
    {
        [JsonProperty(PropertyName = "result")]
        public TRes Result { get; set; }
    }
    
    public class ElementBoundReactResultResponse : ChromeResponse<ScriptExecResult<ElementBoundRectResult>>
    {
        
    }
}