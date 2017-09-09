namespace WikiScreen.Chrome.Requests.Response
{
    public class ElementBoundRect
    {
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }
    
    public class ElementBoundRectResult 
    {
        public string type { get; set; }
        public ElementBoundRect value { get; set; }
    }

    public class ScriptExecResult<TRes>
    {
        public TRes  result { get; set; }
    }
    
    public class ElementBoundReactResultResponse : ChromeResponse<ScriptExecResult<ElementBoundRectResult>>
    {
        
    }
}