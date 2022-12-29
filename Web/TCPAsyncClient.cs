using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Quicksand.Web.Http;
using System;

namespace Quicksand.Web
{
    /// <summary>
    /// Class representing an asynchronous TCP client
    /// </summary>
    public abstract class TCPAsyncClient
    {
        private readonly URL m_URL;
        private AProtocol? m_Protocol = null;
        private readonly Socket m_Socket;
        private Stream m_Stream;
        private byte[] m_Buffer = new byte[1024];
        private readonly bool m_IsSecured = false;
        private readonly bool m_CanConnect;
        private bool m_IsConnected = false;

        /// <returns>The <seealso cref="URL"/> of the client</returns>
        public URL GetURL() => m_URL;

        /// <returns>The <seealso cref="Stream"/> of the client</returns>
        protected Stream GetStream() => m_Stream;

        /// <returns>If the client is a secured connection or not</returns>
        public bool IsSecured() => m_IsSecured;

        /// <returns>If the client is connected or not</returns>
        public bool IsConnected() => m_IsConnected;

        /// <summary>
        /// Set the protocole to read and write from the network
        /// </summary>
        /// <param name="protocole">Protocole to read and write from the network</param>
        protected void SetProtocol(AProtocol protocole)
        {
            m_Protocol = protocole;
            m_Protocol.SetStream(m_Stream);
        }

        /// <summary>
        /// Constructor of a client side TCP async client
        /// </summary>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="isSecured">Specify if we need to use SSL or not</param>
        protected TCPAsyncClient(URL url, bool isSecured)
        {
            m_URL = url;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Stream = Stream.Null;
            m_IsSecured = isSecured;
            m_CanConnect = true;
        }

        /// <summary>
        /// Constructor of a client side TCP async client
        /// </summary>
        /// <param name="url">URL of the server to connect to</param>
        protected TCPAsyncClient(URL url): this(url, url.Scheme switch
        {
            "https" or "wss" => true,
            _ => false
        }) {}

        /// <summary>
        /// Constructor of a server side TCP async client
        /// </summary>
        /// <param name="socket">Socket to use with this client</param>
        /// <param name="certificate">SSL Certificate of the server</param>
        protected TCPAsyncClient(Socket socket, X509Certificate? certificate = null)
        {
            IPEndPoint? endpoint = (IPEndPoint?)socket.RemoteEndPoint;
            m_CanConnect = false;
            m_IsConnected = true;
            m_Socket = socket;
            if (certificate != null)
            {
                SslStream sslStream = new(new NetworkStream(socket), false);
                sslStream.AuthenticateAsServer(certificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                m_Stream = sslStream;
                m_URL = new(string.Format("{0}://{1}:{2}", "https", (endpoint != null) ? endpoint.Address.ToString() : "", (endpoint != null) ? endpoint.Port : -1));
                m_IsSecured = true;
            }
            else
            {
                m_Stream = new NetworkStream(socket);
                m_URL = new(string.Format("{0}://{1}:{2}", "http", (endpoint != null) ? endpoint.Address.ToString() : "", (endpoint != null) ? endpoint.Port : -1));
                m_IsSecured = false;
            }
        }

        /// <summary>
        /// Connect the client to a server
        /// </summary>
        /// <returns>True if it has connect</returns>
        public virtual bool Connect()
        {
            if (!m_CanConnect)
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
                        if (OnConnect())
                        {
                            StartReceiving();
                            m_IsConnected = true;
                            return true;
                        }
                        m_Socket.Close();
                        return false;
                    }
                }
            } catch {}
            return false;
        }

        private async Task Receive()
        {
            byte[] readBuffer = new byte[1024];
            int bytesRead = await m_Stream.ReadAsync(readBuffer.AsMemory(0, readBuffer.Length));
            if (bytesRead >= 1)
            {
                byte[] buffer = new byte[bytesRead];
                for (int i = 0; i < bytesRead; ++i)
                    buffer[i] = readBuffer[i];
                while (bytesRead == readBuffer.Length)
                {
                    int bufferLength = buffer.Length;
                    bytesRead = m_Stream.Read(readBuffer, 0, readBuffer.Length);
                    Array.Resize(ref buffer, bufferLength + bytesRead);
                    for (int i = 0; i < bytesRead; ++i)
                        buffer[i + bufferLength] = readBuffer[i];
                }
                m_Protocol?.ReadBuffer(buffer);
                StartReceiving();
            }
            else
                Disconnect();
        }

        internal void StartReceiving()
        {
            Task.Factory.StartNew(async () => await Receive());
        }

        /// <summary>
        /// Send the given message to the client
        /// </summary>
        /// <param name="message">Message to send to the client/server</param>
        public void Send(string message) => m_Protocol?.WriteBuffer(message);

        /// <summary>
        /// Stop and disconnect the client
        /// </summary>
        public void Disconnect()
        {
            m_IsConnected = false;
            OnDisconnect();
            m_Socket.Close();
            m_Stream.Close();
        }

        /// <summary>
        /// Function called when the client is connected
        /// </summary>
        /// <returns>True if everything went fine</returns>
        protected abstract bool OnConnect();
        /// <summary>
        /// Function called when the client is disconnected
        /// </summary>
        protected abstract void OnDisconnect();
    }
}
