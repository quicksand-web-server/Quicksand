using System.Net;
using System.Net.Sockets;

namespace Quicksand.Web
{
    public class Client: WebSocket.IListener, Http.IListener
    {
        private readonly string m_IP;
        private readonly int m_Port;
        private readonly int m_ID;
        private readonly Socket m_Socket;
        private byte[] m_Buffer = new byte[1024];
        private readonly Server? m_Server;
        private AProtocole m_ReaderWriter;
        private bool m_IsWebSocket = false;

        public Client(string ip, int port)
        {
            m_IP = ip;
            m_Port = port;
            m_ID = 0;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Server = null;
            m_ReaderWriter = new Http.Protocol(m_Socket, this);
        }

        internal Client(Server server, Socket socket, int id)
        {
            m_IP = "";
            m_Port = -1;
            m_Server = server;
            m_Socket = socket;
            m_ID = id;
            m_ReaderWriter = new Http.Protocol(m_Socket, this);
        }

        public bool Connect()
        {
            if (string.IsNullOrWhiteSpace(m_IP))
                throw new Exception("Client can't connect");
            try
            {
                IPEndPoint endpoint = new(m_Port, 11000);
                m_Socket.Connect(endpoint);
                StartReceiving();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("listening error" + ex);
            }
        }

        public bool IsWebSocket() { return m_IsWebSocket; }

        public int GetID() { return m_ID; }

        public void StartReceiving()
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
            m_Server?.RemoveUser(m_ID);
            m_Socket.Disconnect(true);
        }

        public void Send(string response)
        {
            m_ReaderWriter.WriteBuffer(response);
        }

        public void SendResponse(Http.Response? response)
        {
            if (response != null)
                m_ReaderWriter.WriteBuffer(response.ToString());
        }

        public void SendRequest(Http.Request? request)
        {
            if (request != null)
                m_ReaderWriter.WriteBuffer(request.ToString());
        }

        //=====================\\
        // WebSocket.IListener \\
        //=====================\\
        public void OnMessage(string message)
        {
            m_Server?.OnWebsocketMessage(m_ID, message);
        }

        public void OnClose(short code, string _)
        {
            Disconnect();
        }

        public void OnError(string _)
        {
            Disconnect();
        }

        public void OnPing(string message) {}

        public void OnPong(string _) { }

        //================\\
        // Http.IListener \\
        //================\\
        public void OnRequest(Http.Request request)
        {
            if (request.HaveHeaderField("Sec-WebSocket-Key"))
            {
                m_ReaderWriter.WriteBuffer(WebSocket.Protocol.HandleHandshake(request).ToString());
                m_ReaderWriter = new WebSocket.Protocol(m_Socket, this);
                m_IsWebSocket = true;
            }
            m_Server?.TreatRequest(m_ID, request);
        }

        public void OnResponse(Http.Response response)
        {
            if (response.HaveHeaderField("Sec-WebSocket-Accept"))
            {
                //Check Sec-WebSocket-Accept field value
                m_ReaderWriter = new WebSocket.Protocol(m_Socket, this);
                m_IsWebSocket = true;
            }
        }
    }
}
