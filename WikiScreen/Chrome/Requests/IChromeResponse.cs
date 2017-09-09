

using System;

namespace WikiScreen.Chrome.Requests
{
    public interface IChromeResponse
    {
        string method { get; set; }
        int? id { get; set; }  
    }
    
    [Serializable]
    public class ChromeResponse<TRes> : IChromeResponse
    {
        public string method { get; set; }
        public int? id { get; set; }
        public TRes result { get; set; }
    }
    
    
}