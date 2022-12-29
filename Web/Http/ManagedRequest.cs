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
            public override void OnClientDisconnect(int _) {}
            public override void OnHttpResponse(int _, Response response) => m_AutoRequest.SetResponse(response);
        }

        private bool m_IsSent = false;
        private TaskCompletionSource<Response> m_Task = new();
        private readonly Client m_Client;
        private readonly Request m_Request;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestType">Type of the request to send</param>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        /// <exception cref="ArgumentException">Exception thrown if url is invalid</exception>
        protected ManagedRequest(string requestType, string url, string content = "")
        {
            URL requestURL = new(url);
            m_Client = new(new AutoRequestClientListener(this), requestURL);
            m_Client.Connect();
            m_Request = new(requestType, requestURL.Path, content);
            m_Request["Host"] = requestURL.Host;
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

        /// <summary>
        /// Reset the managed request and allow to resend it afterwise
        /// </summary>
        public void Reset()
        {
            m_IsSent = false;
            m_Task = new();
        }

        internal void SetResponse(Response response)
        {
            m_Task.SetResult(response);
        }

        public override string ToString()
        {
            return m_Request.ToString();
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
    /// Class representing a direct HTTP/HTTPS HEAD request
    /// </summary>
    public class HeadRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public HeadRequest(string url, string content = "") : base("HEAD", url, content) { }
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

    /// <summary>
    /// Class representing a direct HTTP/HTTPS PUT request
    /// </summary>
    public class PutRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public PutRequest(string url, string content = "") : base("PUT", url, content) { }
    }

    /// <summary>
    /// Class representing a direct HTTP/HTTPS DELETE request
    /// </summary>
    public class DeleteRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public DeleteRequest(string url, string content = "") : base("DELETE", url, content) { }
    }

    /// <summary>
    /// Class representing a direct HTTP/HTTPS CONNECT request
    /// </summary>
    public class ConnectRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public ConnectRequest(string url, string content = "") : base("CONNECT", url, content) { }
    }

    /// <summary>
    /// Class representing a direct HTTP/HTTPS OPTIONS request
    /// </summary>
    public class OptionsRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public OptionsRequest(string url, string content = "") : base("OPTIONS", url, content) { }
    }

    /// <summary>
    /// Class representing a direct HTTP/HTTPS TRACE request
    /// </summary>
    public class TraceRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public TraceRequest(string url, string content = "") : base("TRACE", url, content) { }
    }

    /// <summary>
    /// Class representing a direct HTTP/HTTPS PATCH request
    /// </summary>
    public class PatchRequest : ManagedRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL of the client http[s]://ip:port/path</param>
        /// <param name="content">Content of the request ro send</param>
        public PatchRequest(string url, string content = "") : base("PATCH", url, content) { }
    }
}
