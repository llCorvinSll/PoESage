using System.Collections.Generic;

namespace WikiScreen.Chrome.Requests
{
    public interface IChromeRequest
    {
        int id { get; set; } 
        string method { get; set; }
        Dictionary<string, dynamic> @params { get; set; }
    }


    public class ChromeRequest : IChromeRequest
    {
        public int id { get; set; }
        public string method { get; set; }
        public Dictionary<string, dynamic> @params { get; set; }
    }
}
