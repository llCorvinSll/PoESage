namespace WikiScreen.Chrome.Requests.Response
{
    public class CaptureScreenshotResult
    {
        public string data { get; set; }
    }
    
    public class CaptureScreenshotResponse : ChromeResponse<CaptureScreenshotResult>
    {
        
    }
}