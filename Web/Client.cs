using Quicksand.Web.Http;
using Quicksand.Web.WebSocket;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Quicksand.Web
{
    /// <summary>
    /// A HTTP/WebSocket Client or Server connection
    /// </summary>
    public class Client: TCPAsyncClient
    {
        private readonly int m_ID;
        /// <summary>
        /// <seealso cref="AClientListener"/> of this client
        /// </summary>
        protected readonly AClientListener m_Listener;
        private bool m_IsWebSocket = false;
        private readonly bool m_IsServer;
        private int m_WebSocketFragmentSize = 0;

        /// <summary>
        /// Constructor of a HTTP/WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, URL url, int id = 0): base(url)
        {
            m_ID = id;
            m_Listener = listener;
            m_IsServer = false;
        }

        /// <summary>
        /// Constructor of a HTTP/WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="ip">IP of the server to connect to</param>
        /// <param name="port">Port of the server to connect to (80 by default)</param>
        /// <param name="isSecured">Specify if his client is connected to an HTTPS server (false by default)</param>
        /// <param name="id">ID of the client (0 by default)</param>
        [Obsolete("Please use URL constructor")]
        public Client(AClientListener listener, string ip, int port = 80, bool isSecured = false, int id = 0):
            this(listener, (isSecured) ? new(string.Format("https://{0}:{1}", ip, port)) : new(string.Format("http://{0}:{1}", ip, port)), id) {}

        internal Client(AClientListener listener, Socket socket, int id, X509Certificate? certificate = null): base(socket, certificate)
        {
            m_ID = id;
            m_Listener = listener;
            m_IsServer = true;
            SetProtocol(new Http.Protocol(this));
        }
        /// <summary>
        /// Function called when the client is connected
        /// </summary>
        /// <returns>True</returns>
        protected override bool OnConnect()
        {
            Logger.Log(Logger.LogType.TCP, string.Format("[{0}] Connected", m_ID));
            SetProtocol(new Http.Protocol(this));
            return true;
        }

        /// <summary>
        /// Function called when the client is disconnected
        /// </summary>
        protected override void OnDisconnect()
        {
            Logger.Log(Logger.LogType.TCP, string.Format("[{0}] Diconnected", m_ID));
            m_Listener.OnClientDisconnect(m_ID);
        }

        /// <summary>
        /// Set the max size before websocket fragment the frames
        /// </summary>
        /// <param name="size">Max size before fragmenting the frames (0 if not fragmentation)</param>
        public void SetWebSocketFragmentSize(int size) => m_WebSocketFragmentSize = size;

        /// <returns>If the client is a websocket or not</returns>
        public bool IsWebSocket() => m_IsWebSocket;

        /// <returns>If the client is a server client (Connection created by the <seealso cref="Server"/>)</returns>
        public bool IsServer() => m_IsServer;

        /// <returns>The ID of the client</returns>
        public int GetID() => m_ID;

        internal void SendResponse(Response? response)
        {
            if (response != null)
            {
                Logger.Log(Logger.LogType.HTTP, string.Format("=> [{0}] {1}", m_ID, response.ToString()));
                Send(response.ToString());
                m_Listener.OnHttpResponseSent(m_ID, response);
            }
        }

        /// <summary>
        /// Send the given HTTP request to the client
        /// </summary>
        /// <param name="request">HTTP request to send to the server</param>
        public void SendRequest(Request? request)
        {
            if (request != null)
            {
                Logger.Log(Logger.LogType.HTTP, string.Format("=> [{0}] {1}", m_ID, request.ToString()));
                Send(request.ToString());
                m_Listener.OnHttpRequestSent(m_ID, request);
            }
        }

        internal void OnMessage(string message) => m_Listener.OnWebSocketMessage(m_ID, message);

        internal void OnClose(short code, string message)
        {
            Disconnect();
            m_Listener.OnWebSocketClose(m_ID, code, message);
        }

        internal void OnError(string error)
        {
            Disconnect();
            m_Listener.OnWebSocketError(m_ID, error);
        }

        /// <summary>
        /// Enable websocket protocol on this client
        /// </summary>
        protected void SetWebSocketProtocol()
        {
            SetProtocol(new WebSocket.Protocol(this, m_WebSocketFragmentSize));
            m_IsWebSocket = true;
        }

        internal virtual void OnRequest(Request request)
        {
            Logger.Log(Logger.LogType.HTTP, string.Format("<= [{0}] {1}", m_ID, request.ToString()));
            if (request.HaveHeaderField("Sec-WebSocket-Key"))
            {
                Send(WebSocket.Protocol.HandleHandshake(request).ToString());
                SetWebSocketProtocol();
            }
            else
                m_Listener.OnHttpRequest(m_ID, request);
        }

        internal virtual void OnResponse(Response response)
        {
            Logger.Log(Logger.LogType.HTTP, string.Format("<= [{0}] {1}", m_ID, response.ToString()));
            m_Listener.OnHttpResponse(m_ID, response);
        }

        internal void OnWebsocketFrameSent(Frame frame) => m_Listener.OnWebSocketFrameSent(m_ID, frame);
        internal void OnWebsocketFrameReceived(Frame frame) => m_Listener.OnWebSocketFrame(m_ID, frame);
    }
}
