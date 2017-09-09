namespace WikiScreen.Chrome.Requests.Response
{
    public class LayoutViewport
    {
        public int pageX { get; set; }
        public int pageY { get; set; }
        public int clientWidth { get; set; }
        public int clientHeight { get; set; }
    }

    public class VisualViewport
    {
        public double offsetX {get;set;}
        public double offsetY {get;set;}
        public double pageX {get;set;}
        public double pageY {get;set;}
        public double clientWidth {get;set;}
        public double clientHeight {get;set;}
        public double scale {get;set;}
    }

    public class ContentSize
    {
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }

    public class GetLayoutMetricsResult
    {
        public LayoutViewport layoutViewport { get; set; }
        
        public VisualViewport visualViewport { get; set; }
        
        public ContentSize contentSize { get; set; }
        
    }

    public class GetLayoutMetricsResponse : ChromeResponse<GetLayoutMetricsResult>
    {
        
    }
}