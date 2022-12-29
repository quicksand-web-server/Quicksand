using Quicksand.Web.Http;
using System.Text;

namespace Quicksand.Web.WebSocket
{
    /// <summary>
    /// A Websocket Client
    /// </summary>
    public class Client : Web.Client
    {
        private readonly string m_SecWebSocketKey = Guid.NewGuid().ToString().Replace("-", "");
        private readonly string m_ExpectedSecWebSocketKey;

        /// <summary>
        /// Constructor of a WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="ip">IP of the server to connect to</param>
        /// <param name="isSecured">Specify if his client is connected to an WSS server</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string ip, bool isSecured, int id = 0) :
            base(listener, (isSecured) ? new(string.Format("wss://{0}", ip)) : new(string.Format("ws://{0}", ip)), id)
        {
            m_ExpectedSecWebSocketKey = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(m_SecWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        }

        /// <summary>
        /// Constructor of a WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string url, int id = 0) :
            base(listener, new(url), id)
        {
            m_ExpectedSecWebSocketKey = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(m_SecWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        }

        /// <summary>
        /// Constructor of a WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, URL url, int id = 0) : base(listener, url, id)
        {
            m_ExpectedSecWebSocketKey = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(m_SecWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        }

        /// <summary>
        /// Connect the client to a server
        /// </summary>
        /// <returns>True if it has connect</returns>
        public override bool Connect() => Connect(new());

        /// <summary>
        /// Connect the client to a server
        /// </summary>
        /// <param name="extensions">Extension header fields to the handshake request</param>
        /// <returns>True if it has connect</returns>
        public bool Connect(Dictionary<string, string> extensions)
        {
            if (base.Connect())
            {
                URL url = GetURL();
                Request handshakeRequest = new("GET", (!string.IsNullOrWhiteSpace(url.Path)) ? url.Path : "/");
                handshakeRequest["Upgrade"] = "websocket";
                handshakeRequest["Connection"] = "Upgrade";
                handshakeRequest["Sec-WebSocket-Version"] = "13";
                handshakeRequest["Sec-WebSocket-Key"] = m_SecWebSocketKey;
                handshakeRequest["Host"] = url.Host;
                foreach (KeyValuePair<string, string> extension in extensions)
                    handshakeRequest[extension.Key] = extension.Value;
                SendRequest(handshakeRequest);
                return true;
            }
            return false;
        }

        internal override void OnResponse(Response response)
        {
            if (response.HaveHeaderField("Sec-WebSocket-Accept"))
            {
                if ((string)response["Sec-WebSocket-Accept"] == m_ExpectedSecWebSocketKey)
                {
                    SetWebSocketProtocol();
                    m_Listener.OnWebSocketOpen(GetID(), response);
                }
            }
            else
                m_Listener.OnHttpResponse(GetID(), response);
        }
    }
}
