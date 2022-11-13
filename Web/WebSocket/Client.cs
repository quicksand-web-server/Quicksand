namespace Quicksand.Web.WebSocket
{
    /// <summary>
    /// A Websocket Client
    /// </summary>
    public class Client : Web.Client
    {
        /// <summary>
        /// Constructor of a WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="ip">IP of the server to connect to</param>
        /// <param name="isSecured">Specify if his client is connected to an WSS server</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string ip, bool isSecured, int id = 0) :
            base(listener, (isSecured) ? new(string.Format("wss://{0}", ip)) : new(string.Format("ws://{0}", ip)), id) { }

        /// <summary>
        /// Constructor of a WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string url, int id = 0) :
            base(listener, new(url), id) { }

        /// <summary>
        /// Constructor of a WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, URL url, int id = 0) : base(listener, url, id) {}

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
                SendWebSocketHandshake(extensions);
                return true;
            }
            return false;
        }
    }
}
