using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Quicksand.Web
{
    /// <summary>
    /// A HTTP/WebSocket Client or Server connection
    /// </summary>
    public class Client
    {
        private readonly string m_IP;
        private readonly int m_Port;
        private readonly int m_ID;
        private readonly Socket m_Socket;
        private Stream m_Stream;
        private byte[] m_Buffer = new byte[1024];
        private readonly AClientListener m_Listener;
        private AProtocole m_ReaderWriter;
        private bool m_IsWebSocket = false;
        private readonly bool m_IsSecured = false;
        private readonly bool m_CanConnect;

        /// <summary>
        /// Constructor of a HTTP/WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="ip">IP of the server to connect to</param>
        /// <param name="port">Port of the server to connect to (80 by default)</param>
        /// <param name="isSecured">Specify if his client is connected to an HTTPS server (false by default)</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string ip, int port = 80, bool isSecured = false, int id = 0)
        {
            m_IP = ip;
            m_Port = port;
            m_ID = id;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Stream = Stream.Null;
            m_Listener = listener;
            m_ReaderWriter = new Http.Protocol(m_Stream, this);
            m_CanConnect = true;
            m_IsSecured = isSecured;
        }

        internal Client(AClientListener listener, Socket socket, int id, X509Certificate? certificate = null)
        {
            IPEndPoint? endpoint = (IPEndPoint?)socket.RemoteEndPoint;
            m_IP = (endpoint != null) ? endpoint.Address.ToString() : "";
            m_Port = (endpoint != null) ? endpoint.Port : - 1;
            m_ID = id;
            m_Socket = socket;
            if (certificate != null)
            {
                SslStream sslStream = new(new NetworkStream(socket), false);
                sslStream.AuthenticateAsServer(certificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                m_Stream = sslStream;
                m_IsSecured = true;
            }
            else
            {
                m_Stream = new NetworkStream(socket);
                m_IsSecured = false;
            }
            m_Listener = listener;
            m_ReaderWriter = new Http.Protocol(m_Stream, this);
            m_CanConnect = false;
        }

        /// <summary>
        /// Connect the lient to a server
        /// </summary>
        /// <returns>True if it has connect</returns>
        public bool Connect()
        {
            if (!m_CanConnect)
                return false;
            try
            {
                foreach (IPAddress ip in Dns.GetHostEntry(m_IP).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        IPEndPoint endpoint = new(ip, m_Port);
                        m_Socket.Connect(endpoint);
                        m_Stream = new NetworkStream(m_Socket);
                        if (m_IsSecured)
                        {
                            SslStream sslStream = new(m_Stream, leaveInnerStreamOpen: false);
                            var options = new SslClientAuthenticationOptions()
                            {
                                TargetHost = m_IP,
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
                if (bytesRead > 1)
                {
                    Array.Resize(ref m_Buffer, bytesRead);
                    m_ReaderWriter.ReadBuffer(m_Buffer);
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

        internal void Disconnect()
        {
            m_Listener.ClientDisconnect(m_ID);
            m_Stream.Close();
            m_Socket.Disconnect(true);
        }

        /// <summary>
        /// Send the given message to the client
        /// </summary>
        /// <param name="message">Message to send to the client/server</param>
        public void Send(string message) => m_ReaderWriter.WriteBuffer(message);

        /// <summary>
        /// Send the given HTTP response to the client
        /// </summary>
        /// <param name="response">HTTP response to send to the client</param>
        public void SendResponse(Http.Response? response)
        {
            if (response != null)
                m_ReaderWriter.WriteBuffer(response.ToString());
        }

        /// <summary>
        /// Send the given HTTP request to the client
        /// </summary>
        /// <param name="request">HTTP request to send to the server</param>
        public void SendRequest(Http.Request? request)
        {
            if (request != null)
                m_ReaderWriter.WriteBuffer(request.ToString());
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

        internal void OnResponse(Http.Response response)
        {
            if (response.HaveHeaderField("Sec-WebSocket-Accept"))
            {
                //Check Sec-WebSocket-Accept field value
                m_ReaderWriter = new WebSocket.Protocol(m_Stream, this);
                m_IsWebSocket = true;
            }
            else
                m_Listener.HttpResponse(m_ID, response);
        }
    }
}
