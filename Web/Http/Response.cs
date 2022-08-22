using System.Text;

namespace Quicksand.Web.Http
{
    /// <summary>
    /// An HTTP response
    /// </summary>
    public class Response
    {
        private readonly string m_Version;
        private readonly int m_StatusCode;
        private readonly string m_StatusMessage;
        private readonly HeaderFields m_HeaderFields = new();
        private string m_Body = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="response">Content of the received HTTP response</param>
        public Response(string response)
        {
            List<string> attributes = response.Split(new string[] { Defines.CRLF }, StringSplitOptions.None).ToList();
            string[] statusLine = attributes[0].Trim().Split(' ');
            m_Version = statusLine[0];
            m_StatusCode = Int32.Parse(statusLine[1]);
            m_StatusMessage = statusLine[2];
            attributes.RemoveAt(0);
            m_HeaderFields = new(attributes);
        }

        internal void SetBody(string body)
        {
            m_Body = body;
            if (m_Body.Length > 0)
            {
                m_HeaderFields["Accept-Ranges"] = "bytes";
                m_HeaderFields["Content-Length"] = Encoding.UTF8.GetBytes(m_Body).Length;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="statusCode">Status code of the response</param>
        /// <param name="statusMessage">Status message of the response</param>
        /// <param name="body">Body of the response (empty by default)</param>
        /// <param name="contentType">MIME type of the content to send (empty by default)</param>
        public Response(int statusCode, string statusMessage, string body = "", string contentType = "")
        {
            m_Version = Defines.VERSION;
            m_StatusCode = statusCode;
            m_StatusMessage = statusMessage;
            SetBody(body);
            m_HeaderFields["Server"] = "Quicksand HTTP Server";
            if (!string.IsNullOrWhiteSpace(body))
                m_HeaderFields["Content-Type"] = string.Format("{0}; charset=utf-8", contentType);
        }

        /// <returns>The formatted request</returns>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append(m_Version);
            builder.Append(' ');
            builder.Append(m_StatusCode);
            builder.Append(' ');
            builder.Append(m_StatusMessage);
            builder.Append(Defines.CRLF);
            builder.Append(m_HeaderFields);
            builder.Append(m_Body);
            return builder.ToString();
        }

        /// <summary>
        /// Check if the response contains the given header field
        /// </summary>
        /// <param name="headerFieldName">Name of the header field to search</param>
        /// <returns>True if the header field exists</returns>
        public bool HaveHeaderField(string headerFieldName) { return m_HeaderFields.HaveHeaderField(headerFieldName); }

        /// <summary>
        /// HTTP version of the response
        /// </summary>
        public string Version { get => m_Version; }

        /// <summary>
        /// Status code of the response
        /// </summary>
        public int StatusCode { get => m_StatusCode; }

        /// <summary>
        /// Status message of the response
        /// </summary>
        public string StatusMessage { get => m_StatusMessage; }

        /// <summary>
        /// Body of the response
        /// </summary>
        public string Body { get => m_Body; }

        /// <summary>
        /// Header field accessors
        /// </summary>
        /// <param name="key">Name of the header field to get/set</param>
        /// <returns>The content of the header field</returns>
        public object this[string key]
        {
            get => m_HeaderFields[key];
            set => m_HeaderFields[key] = value;
        }
    }
}
