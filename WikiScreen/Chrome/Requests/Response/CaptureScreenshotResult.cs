using Newtonsoft.Json;

namespace WikiScreen.Chrome.Requests.Response
{
    public class CaptureScreenshotResult
    {
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
    }
    
    public class CaptureScreenshotResponse : ChromeResponse<CaptureScreenshotResult>
    {
        
    }
}