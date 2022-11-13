using Quicksand.Web.Http;
using Quicksand.Web.WebSocket;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Quicksand.Web
{
    /// <summary>
    /// A HTTP/WebSocket Client or Server connection
    /// </summary>
    public class Client
    {
        private readonly string m_SecWebSocketKey = Guid.NewGuid().ToString();
        private readonly string m_ExpectedSecWebSocketKey;
        private readonly URL m_URL;
        private readonly int m_ID;
        private readonly Socket m_Socket;
        private Stream m_Stream;
        private byte[] m_Buffer = new byte[1024];
        private readonly AClientListener m_Listener;
        private AProtocole m_ReaderWriter;
        private bool m_IsWebSocket = false;
        private readonly bool m_IsSecured = false;
        private readonly bool m_IsServer;

        private static bool IsSecuredScheme(string scheme) => scheme switch
        {
            "https" or "wss" => true,
            _ => false
        };

        /// <summary>
        /// Constructor of a HTTP/WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, URL url, int id = 0)
        {
            m_URL = url;
            m_ID = id;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Stream = Stream.Null;
            m_Listener = listener;
            m_ReaderWriter = new Http.Protocol(m_Stream, this);
            m_IsServer = false;
            m_IsSecured = IsSecuredScheme(url.Scheme);
            m_SecWebSocketKey = Guid.NewGuid().ToString().Replace("-", "");
            m_ExpectedSecWebSocketKey = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(m_SecWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
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

        internal Client(AClientListener listener, Socket socket, int id, X509Certificate? certificate = null)
        {
            IPEndPoint? endpoint = (IPEndPoint?)socket.RemoteEndPoint;
            m_ID = id;
            m_Socket = socket;
            if (certificate != null)
            {
                SslStream sslStream = new(new NetworkStream(socket), false);
                sslStream.AuthenticateAsServer(certificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                m_Stream = sslStream;
                m_URL = new(string.Format("https://{0}:{1}", (endpoint != null) ? endpoint.Address.ToString() : "", (endpoint != null) ? endpoint.Port : -1));
                m_IsSecured = true;
            }
            else
            {
                m_Stream = new NetworkStream(socket);
                m_URL = new(string.Format("http://{0}:{1}", (endpoint != null) ? endpoint.Address.ToString() : "", (endpoint != null) ? endpoint.Port : -1));
                m_IsSecured = false;
            }
            m_Listener = listener;
            m_ReaderWriter = new Http.Protocol(m_Stream, this);
            m_IsServer = true;
            m_SecWebSocketKey = "";
            m_ExpectedSecWebSocketKey = "";
        }

        /// <summary>
        /// Connect the client to a server
        /// </summary>
        /// <returns>True if it has connect</returns>
        public virtual bool Connect()
        {
            if (m_IsServer)
                return false;
            try
            {
                foreach (IPAddress ip in Dns.GetHostEntry(m_URL.Host).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        IPEndPoint endpoint = new(ip, m_URL.Port);
                        m_Socket.Connect(endpoint);
                        m_Stream = new NetworkStream(m_Socket);
                        if (m_IsSecured)
                        {
                            SslStream sslStream = new(m_Stream, leaveInnerStreamOpen: false);
                            var options = new SslClientAuthenticationOptions()
                            {
                                TargetHost = m_URL.Host,
                                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None,
                            };

                            sslStream.AuthenticateAsClientAsync(options, CancellationToken.None).Wait();
                            m_Stream = sslStream;
                        }
                        m_ReaderWriter = new Http.Protocol(m_Stream, this);
                        StartReceiving();
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
        }

        /// <returns>If the client is a websocket or not</returns>
        public bool IsWebSocket() => m_IsWebSocket;

        /// <returns>If the client is a secured connection or not</returns>
        public bool IsSecured() => m_IsSecured;

        /// <returns>If the client is a server client (Connection created by the <seealso cref="Server"/>)</returns>
        public bool IsServer() => m_IsServer;

        /// <returns>The ID of the client</returns>
        public int GetID() => m_ID;

        internal void StartReceiving()
        {
            try
            {
                m_Buffer = new byte[1024];
                m_Stream.BeginRead(m_Buffer, 0, m_Buffer.Length, ReceiveCallback, null);
            }
            catch { }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int bytesRead = m_Stream.EndRead(AR);
                if (bytesRead >= 1)
                {
                    byte[] buffer = new byte[bytesRead];
                    for (int i = 0; i < bytesRead; ++i)
                        buffer[i] = m_Buffer[i];
                    while (bytesRead == m_Buffer.Length)
                    {
                        int bufferLength = buffer.Length;
                        bytesRead = m_Stream.Read(m_Buffer, 0, m_Buffer.Length);
                        Array.Resize(ref buffer, bufferLength + bytesRead);
                        for (int i = 0; i < bytesRead; ++i)
                            buffer[i + bufferLength] = m_Buffer[i];
                    }
                    m_ReaderWriter.ReadBuffer(buffer);
                    StartReceiving();
                }
                else
                    Disconnect();
            }
            catch
            {
                if (!m_Socket.Connected)
                    Disconnect();
                else
                    StartReceiving();
            }
        }

        /// <summary>
        /// Stop and disconnect the client
        /// </summary>
        public void Disconnect()
        {
            m_Listener.ClientDisconnect(m_ID);
            m_Socket.Close();
            m_Stream.Close();
        }

        /// <summary>
        /// Send the given message to the client
        /// </summary>
        /// <param name="message">Message to send to the client/server</param>
        public void Send(string message) => m_ReaderWriter.WriteBuffer(message);

        internal void SendResponse(Response? response)
        {
            if (response != null)
            {
                m_ReaderWriter.WriteBuffer(response.ToString());
                m_Listener.HttpResponseSent(m_ID, response);
            }
        }

        /// <summary>
        /// Send the given HTTP request to the client
        /// </summary>
        /// <param name="request">HTTP request to send to the server</param>
        public void SendRequest(Http.Request? request)
        {
            if (request != null)
            {
                m_ReaderWriter.WriteBuffer(request.ToString());
                m_Listener.HttpRequestSent(m_ID, request);
            }
        }

        internal void OnMessage(string message) => m_Listener.WebSocketMessage(m_ID, message);

        internal void OnClose(short code, string message)
        {
            Disconnect();
            m_Listener.WebSocketClose(m_ID, code, message);
        }

        internal void OnError(string error)
        {
            Disconnect();
            m_Listener.WebSocketError(m_ID, error);
        }

        internal void OnRequest(Http.Request request)
        {
            if (request.HaveHeaderField("Sec-WebSocket-Key"))
            {
                m_ReaderWriter.WriteBuffer(WebSocket.Protocol.HandleHandshake(request).ToString());
                m_ReaderWriter = new WebSocket.Protocol(m_Stream, this);
                m_IsWebSocket = true;
            }
            else
                m_Listener.HttpRequest(m_ID, request);
        }

        /// <summary>
        /// Send the WebSocket handshake to the server
        /// </summary>
        [Obsolete("Use the Websocket.Client")]
        public void SendWebSocketHandshake() => SendWebSocketHandshake(new());

        /// <summary>
        /// Send the WebSocket handshake to the server
        /// </summary>
        /// <param name="extensions">Extension header fields to the handshake request</param>
        protected void SendWebSocketHandshake(Dictionary<string, string> extensions)
        {
            Request handshakeRequest = new("GET", (!string.IsNullOrWhiteSpace(m_URL.Path)) ? m_URL.Path : "/");
            handshakeRequest["Upgrade"] = "websocket";
            handshakeRequest["Connection"] = "Upgrade";
            handshakeRequest["Sec-WebSocket-Version"] = "13";
            handshakeRequest["Sec-WebSocket-Key"] = m_SecWebSocketKey;
            handshakeRequest["Host"] = m_URL.Host;
            foreach (KeyValuePair<string, string> extension in extensions)
                handshakeRequest[extension.Key] = extension.Value;
            SendRequest(handshakeRequest);
        }

        internal void OnResponse(Response response)
        {
            if (response.HaveHeaderField("Sec-WebSocket-Accept"))
            {
                if ((string)response["Sec-WebSocket-Accept"] == m_ExpectedSecWebSocketKey)
                {
                    m_ReaderWriter = new WebSocket.Protocol(m_Stream, this);
                    m_IsWebSocket = true;
                    m_Listener.WebSocketOpen(m_ID, response);
                }
            }
            else
                m_Listener.HttpResponse(m_ID, response);
        }

        internal void OnWebsocketFrameSent(Frame frame) => m_Listener.WebSocketFrameSent(m_ID, frame);
        internal void OnWebsocketFrameReceived(Frame frame) => m_Listener.WebSocketFrame(m_ID, frame);
    }
}
