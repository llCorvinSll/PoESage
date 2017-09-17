using Newtonsoft.Json;

namespace WikiScreen.Chrome.Requests.Response
{
    public class LayoutViewport
    {
        [JsonProperty(PropertyName = "pageX")]
        public int? PageX { get; set; }
        
        [JsonProperty(PropertyName = "pageY")]
        public int? PageY { get; set; }
        
        [JsonProperty(PropertyName = "clientWidth")]
        public int? ClientWidth { get; set; }
        
        [JsonProperty(PropertyName = "clientHeight")]
        public int? ClientHeight { get; set; }
    }

    public class VisualViewport
    {
        [JsonProperty(PropertyName = "offsetX")]
        public double? OffsetX {get;set;}
        
        [JsonProperty(PropertyName = "offsetY")]
        public double? OffsetY {get;set;}
        
        [JsonProperty(PropertyName = "pageX")]
        public double? PageX {get;set;}
        
        [JsonProperty(PropertyName = "pageY")]
        public double? PageY {get;set;}
        
        [JsonProperty(PropertyName = "clientWidth")]
        public double? ClientWidth {get;set;}
        
        [JsonProperty(PropertyName = "clientHeight")]
        public double? ClientHeight {get;set;}
        
        [JsonProperty(PropertyName = "scale")]
        public double? Scale {get;set;}
    }

    public class ContentSize
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

    public class GetLayoutMetricsResult
    {  
        [JsonProperty(PropertyName = "layoutViewport")]
        public LayoutViewport LayoutViewport { get; set; }
        
        [JsonProperty(PropertyName = "visualViewport")]
        public VisualViewport VisualViewport { get; set; }
        
        [JsonProperty(PropertyName = "contentSize")]
        public ContentSize ContentSize { get; set; }
        
    }

    public class GetLayoutMetricsResponse : ChromeResponse<GetLayoutMetricsResult>
    {
        
    }
}