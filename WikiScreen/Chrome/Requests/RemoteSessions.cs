using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WikiScreen.Chrome.Requests
{
    [Serializable]
    public class RemoteSessions : IChromeRequest
    {
        public int id { get; set; }
        public string method { get; set; }
        public Dictionary<string, dynamic> @params { get; set; }
    }
    
    [Serializable]
    [DataContract]
    public class RemoteSessionsResponse
    {
        public RemoteSessionsResponse() { }

        [DataMember]
        public string devtoolsFrontendUrl;

        [DataMember]
        public string faviconUrl;

        [DataMember]
        public string thumbnailUrl;

        [DataMember]
        public string title;
        
        [DataMember]
        public string url;

        [DataMember]
        public string webSocketDebuggerUrl;
    }
}