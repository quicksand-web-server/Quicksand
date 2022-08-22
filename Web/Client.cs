using System.Net;
using System.Net.Sockets;

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
        private byte[] m_Buffer = new byte[1024];
        private readonly AClientListener m_Listener;
        private AProtocole m_ReaderWriter;
        private bool m_IsWebSocket = false;
        private readonly bool m_CanConnect;

        /// <summary>
        /// Constructor of a HTTP/WebSocket Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="ip">IP of the server to connect to</param>
        /// <param name="port">Port of the server to connect to (80 by default)</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string ip, int port = 80, int id = 0)
        {
            m_IP = ip;
            m_Port = port;
            m_ID = id;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Listener = listener;
            m_ReaderWriter = new Http.Protocol(m_Socket, this);
            m_CanConnect = true;
        }

        internal Client(AClientListener listener, Socket socket, int id)
        {
            IPEndPoint? endpoint = (IPEndPoint?)socket.RemoteEndPoint;
            m_IP = (endpoint != null) ? endpoint.Address.ToString() : "";
            m_Port = (endpoint != null) ? endpoint.Port : - 1;
            m_ID = id;
            m_Socket = socket;
            m_Listener = listener;
            m_ReaderWriter = new Http.Protocol(m_Socket, this);
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
                        StartReceiving();
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
        }

        /// <returns>If the client is a websocket or not</returns>
        public bool IsWebSocket() { return m_IsWebSocket; }

        /// <returns>The ID of the client</returns>
        public int GetID() { return m_ID; }

        internal void StartReceiving()
        {
            try
            {
                m_Buffer = new byte[1024];
                m_Socket.BeginReceive(m_Buffer, 0, m_Buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch { }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int bytesRead = m_Socket.EndReceive(AR);
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
            m_Socket.Disconnect(true);
        }

        /// <summary>
        /// Send the given message to the client
        /// </summary>
        /// <param name="message">Message to send to the client/server</param>
        public void Send(string message)
        {
            m_ReaderWriter.WriteBuffer(message);
        }

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

        internal void OnMessage(string message)
        {
            m_Listener.WebSocketMessage(m_ID, message);
        }

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
                m_ReaderWriter = new WebSocket.Protocol(m_Socket, this);
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
                m_ReaderWriter = new WebSocket.Protocol(m_Socket, this);
                m_IsWebSocket = true;
            }
            else
                m_Listener.HttpResponse(m_ID, response);
        }
    }
}
