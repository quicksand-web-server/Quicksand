namespace Quicksand.Web.Http
{
    /// <summary>
    /// Class representing an HTTP/HTTPS request with response
    /// </summary>
    public class ManagedRequest
    {
        private class AutoRequestClientListener : AClientListener
        {
            private readonly ManagedRequest m_AutoRequest;
            public AutoRequestClientListener(ManagedRequest autoRequest) => m_AutoRequest = autoRequest;
            protected override void OnClientDisconnect(int _) {}
            protected override void OnHttpResponse(int _, Response response) => m_AutoRequest.SetResponse(response);
        }

        private bool m_IsSent = false;
        private readonly Client m_Client;
        private readonly Request m_Request;
        private readonly TaskCompletionSource<Response> m_Task = new();

        internal ManagedRequest(string requestType, string url, string content = "")
        {
            string ip;
            int port;
            bool isSecure;
            string path = "/";
            if (url.StartsWith("http://"))
            {
                ip = url[7..];
                port = 80;
                isSecure = false;
            }
            else if (url.StartsWith("https://"))
            {
                ip = url[8..];
                port = 443;
                isSecure = true;
            }
            else
                throw new ArgumentException(string.Format("Bad URL {0}", url));

            int separatorIdx = ip.IndexOf('/');
            if (separatorIdx > 0)
            {
                path = ip[separatorIdx..];
                ip = ip[..separatorIdx];
            }

            int portIdx = ip.IndexOf(':');
            if (portIdx > 0)
            {
                port = int.Parse(ip[(portIdx + 1)..]);
                ip = ip[..portIdx];
            }

            m_Client = new(new AutoRequestClientListener(this), ip, port, isSecure);
            m_Client.Connect();
            m_Request = new(requestType, path, content);
            m_Request["Host"] = ip;
        }

        /// <summary>
        /// Add an header field to the request to send
        /// </summary>
        /// <param name="fieldName">Name of the header field</param>
        /// <param name="fieldValue">Value of the header field</param>
        /// <returns>This <seealso cref="ManagedRequest"/></returns>
        public ManagedRequest AddHeaderField(string fieldName, string fieldValue)
        {
            m_Request[fieldName] = fieldValue;
            return this;
        }

        /// <summary>
        /// Send the request to the client
        /// </summary>
        /// <param name="timeout">Time in seconds after which send will automatically return if no response has been received</param>
        /// <returns>The client response, null if no response was received</returns>
        public Response? Send(int timeout = 30)
        {
            if (m_IsSent)
            {
                if (m_Task.Task.IsCompleted)
                    return m_Task.Task.Result;
                return null;
            }
            m_IsSent = true;
            m_Client.SendRequest(m_Request);
            if (m_Task.Task.Wait(TimeSpan.FromSeconds(timeout)))
                return m_Task.Task.Result;
            return null;
        }

        internal void SetResponse(Response response)
        {
            m_Task.SetResult(response);
        }
    }

    /// <summary>
    /// Class representing a direct HTTP/HTTPS GET request
    /// </summary>
    public class GetRequest: ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public GetRequest(string url, string content = ""): base("GET", url, content) {}
    }

    /// <summary>
    /// Class representing a direct HTTP/HTTPS POST request
    /// </summary>
    public class PostRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public PostRequest(string url, string content = "") : base("POST", url, content) { }
    }
}
