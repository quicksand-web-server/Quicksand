using System.Text;

namespace Quicksand.Web
{
    /// <summary>
    /// Class representing an URL
    /// </summary>
    public class URL
    {
        /// <summary>
        /// Class representing an URL exception
        /// </summary>
        public class URLException: Exception
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="message">Exception message</param>
            public URLException(string message) : base(message) {}
        }

        private class Authority
        {
            private readonly string m_UserInfo;
            private readonly string m_Host;
            private readonly int m_Port;

            public string UserInfo => m_UserInfo;
            public string Host => m_Host;
            public int Port => m_Port;

            public Authority(string userInfo, string host, int port)
            {
                m_UserInfo = userInfo;
                m_Host = host;
                m_Port = port;
            }

            public override string ToString()
            {
                StringBuilder builder = new();
                if (!string.IsNullOrWhiteSpace(m_UserInfo))
                {
                    builder.Append(m_UserInfo);
                    builder.Append('@');
                }
                builder.Append(m_Host);
                if (m_Port != -1)
                {
                    builder.Append(':');
                    builder.Append(m_Port);
                }
                return builder.ToString();
            }
        }

        private readonly static Dictionary<string, int> ms_DefaultPorts = new()
        {
            { "http", 80 },
            { "ws", 80 },
            { "https", 443 },
            { "wss", 443 },
        };

        private readonly string m_Scheme;
        private readonly Authority? m_Authority = null;
        private readonly string m_Path;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">String containing the URL</param>
        public URL(string url)
        {
            int tmp = url.IndexOf(':');
            if (tmp <= 0)
                throw new URLException("No scheme in the URL");
            m_Scheme = url[..tmp];
            url = url[(tmp + 1)..];
            if (url.StartsWith("//"))
            {
                url = url[2..];
                tmp = url.IndexOf('/');
                string authority;
                if (tmp > 0)
                {
                    authority = url[..tmp];
                    url = url[tmp..];
                }
                else
                {
                    authority = url;
                    url = "";
                }

                int port = -1;
                tmp = authority.IndexOf(':');
                if (tmp > 0)
                {
                    port = int.Parse(authority[(tmp + 1)..]);
                    authority = authority[..tmp];
                }
                else
                    ms_DefaultPorts.TryGetValue(m_Scheme, out port);

                string userInfo = "";
                tmp = authority.IndexOf('@');
                if (tmp > 0)
                {
                    userInfo = authority[tmp..];
                    authority = authority[..(tmp + 1)];
                }

                m_Authority = new(userInfo, authority, port);
            }
            m_Path = url;
        }

        /// <summary>
        /// Scheme of the URL
        /// </summary>
        public string Scheme => m_Scheme;
        /// <summary>
        /// Userinfo of the URL
        /// </summary>
        public string UserInfo => (m_Authority != null) ? m_Authority.UserInfo : "";
        /// <summary>
        /// Host of the URL
        /// </summary>
        public string Host => (m_Authority != null) ? m_Authority.Host : "";
        /// <summary>
        /// Port of the URL
        /// </summary>
        public int Port => (m_Authority != null) ? m_Authority.Port : -1;
        /// <summary>
        /// Path of the URL
        /// </summary>
        public string Path => m_Path;

        /// <returns>The URL string</returns>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append(m_Scheme);
            builder.Append(':');
            if (m_Authority != null)
            {
                builder.Append("//");
                builder.Append(m_Authority.ToString());
            }
            if (!string.IsNullOrWhiteSpace(m_Path))
                builder.Append(m_Path);
            return builder.ToString();
        }
    }
}
