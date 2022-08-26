using System.Text;

namespace Quicksand.Web.Http
{
    /// <summary>
    /// An HTTP request
    /// </summary>
    public class Request
    {
        private readonly string m_Method;
        private readonly string m_Path;
        private readonly string m_Version;
        private readonly HeaderFields m_HeaderFields; //All header fields value will be stored as string in request
        private readonly Dictionary<string, string> m_Parameters = new();
        private string m_Body = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request">Content of the received HTTP request</param>
        public Request(string request)
        {
            List<string> attributes = request.Split(new string[] { Defines.CRLF }, StringSplitOptions.None).ToList();
            string[] requestLine = attributes[0].Trim().Split(' ');
            m_Method = requestLine[0];
            m_Version = requestLine[2];
            attributes.RemoveAt(0);
            m_HeaderFields = new(attributes);
            string path = requestLine[1];
            int parametersIdx = path.IndexOf('?');
            if (parametersIdx < 0)
                m_Path = path;
            else
            {
                m_Path = path[..parametersIdx];
                string parameterLine = path[(parametersIdx + 1)..];
                string[] parameters = parameterLine.Split('&');
                foreach (string parameter in parameters)
                {
                    string[] tmp = parameter.Split('=');
                    if (tmp.Length == 2)
                        m_Parameters[tmp[0]] = tmp[1];
                }
            }

            if (m_Path[^1] == '/')
                m_Path = m_Path[0..^1];
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
        /// <param name="method">Method of the request</param>
        /// <param name="path">URL targeted by the request</param>
        /// <param name="body">Body of the request</param>
        public Request(string method, string path, string body = "")
        {
            m_Method = method;
            m_Path = path;
            m_Version = Defines.VERSION;
            m_HeaderFields = new();
            SetBody(body);
        }



        /// <returns>The formatted request</returns>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append(m_Method);
            builder.Append(' ');
            builder.Append(m_Path);
            builder.Append(' ');
            builder.Append(m_Version);
            builder.Append(Defines.CRLF);
            builder.Append(m_HeaderFields);
            builder.Append(m_Body);
            return builder.ToString();
        }

        /// <summary>
        /// Check if the request contains the given header field
        /// </summary>
        /// <param name="headerFieldName">Name of the header field to search</param>
        /// <returns>True if the header field exists</returns>
        public bool HaveHeaderField(string headerFieldName) { return m_HeaderFields.HaveHeaderField(headerFieldName); }

        /// <summary>
        /// Check if the request contains the given URL parameter
        /// </summary>
        /// <param name="parameterName">Name of the URL parameter to search</param>
        /// <returns>True if the URL parameter exists</returns>
        public bool HaveParameter(string parameterName) { return m_Parameters.ContainsKey(parameterName); }

        /// <summary>
        /// Get the URL parameter value of the given parameter
        /// </summary>
        /// <param name="parameterName">Name of the URL parameter to search</param>
        /// <returns>The value of the URL parameter</returns>
        public string GetParameter(string parameterName) { return m_Parameters[parameterName]; }

        /// <summary>
        /// Get the URL parameter value of the given parameter if it exist
        /// </summary>
        /// <param name="parameterName">Name of the URL parameter to search</param>
        /// <param name="value">Container for the value of the parameter if found</param>
        /// <returns>True if it found a value to the given parameter</returns>
        public bool TryGetParameter(string parameterName, out string? value)
        {
            return m_Parameters.TryGetValue(parameterName, out value);
        }

        /// <summary>
        /// Method of the request
        /// </summary>
        public string Method { get => m_Method; }
        /// <summary>
        /// URL targeted by the request
        /// </summary>
        public string Path { get => m_Path; }
        /// <summary>
        /// HTTP version of the request
        /// </summary>
        public string Version { get => m_Version; }
        /// <summary>
        /// Body of the request
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
