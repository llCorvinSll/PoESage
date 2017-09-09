

using System;

namespace WikiScreen.Chrome.Requests
{
    public interface IChromeResponse
    {
        string method { get; set; }
         int? id { get; set; }
         dynamic result { get; set; }
        
    }
    
    [Serializable]
    public class ChromeResponse : IChromeResponse
    {
        public string method { get; set; }
        public int? id { get; set; }
        public dynamic result { get; set; }
    }
}