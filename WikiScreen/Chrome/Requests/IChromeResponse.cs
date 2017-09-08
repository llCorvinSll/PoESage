

using System;

namespace WikiScreen.Chrome.Requests
{
    public interface IChromeResponse
    {
         int? id { get; set; }
         dynamic result { get; set; }
        
    }
    
    [Serializable]
    public class ChromeResponse : IChromeResponse
    {
        public int? id { get; set; }
        public dynamic result { get; set; }
    }
}