using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Quicksand.Web
{
    /// <summary>
    /// A webserver that can also handle websocket connection
    /// </summary>
    public class Server: AClientListener
    {
        /// <summary>
        /// Handle the method from the request on the resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="clientID">ID of the client which send the request</param>
        /// <param name="request">Request to handle</param>
        /// <returns>True if the method is allowed on the resource and has been handled</returns>
        public delegate bool MethodHandler(Http.Resource resource, int clientID, Http.Request request);

        private readonly Socket m_HttpServerSocket;
        private readonly int m_Port;
        private volatile bool m_Running = false;
        private readonly ClientManager m_ClientManager;
        private readonly ResourceManager m_ResourceManager;
        private readonly Dictionary<string, MethodHandler> m_MethodHandlers = new();
        private X509Certificate? m_ServerCertificate = null;

        /// <summary>
        /// Create a Quicksand server on the specified <paramref name="port"/>
        /// </summary>
        /// <param name="port">Port on which the server will listen (80 by default)</param>
        public Server(int port = 80)
        {
            m_ClientManager = new();
            m_ResourceManager = new(m_ClientManager);
            m_Port = port;
            m_MethodHandlers["GET"] = (Http.Resource resource, int clientID, Http.Request request) => { resource.CallGet(clientID, request); return true; };
            m_HttpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        internal Server(ClientManager clientManager, ResourceManager resourceManager, int port = 80) : this(port)
        {
            m_ClientManager = clientManager;
            m_ResourceManager = resourceManager;
        }

        /// <summary>
        /// Load an SSL certificate file on the server
        /// </summary>
        /// <remarks>
        /// Calling this function will make this http/ws server a https/wss server
        /// </remarks>
        /// <param name="path">Path of the SSL certificate file to use on the server</param>
        public void LoadCertificate(string path) => m_ServerCertificate = X509Certificate.CreateFromCertFile(path);

        /// <returns>The <seealso cref="ResourceManager"/> of this server</returns>
        public ResourceManager GetResourceManager() => m_ResourceManager;

        /// <summary>
        /// Add file pointed by the path as a resource
        /// </summary>
        /// <param name="url">Path of the file in the server. Should start with a /</param>
        /// <param name="path">Path of the file to add</param>
        /// <param name="contentType">MIME type of the content to send</param>
        /// <param name="preLoad">Specify if we should load in memory the file content (false by default)</param>
        [Obsolete("Function will be removed in next version. Please use GetResourceManager instead")]
        public void AddFile(string url, string path, Http.MIME contentType, bool preLoad = false) => m_ResourceManager.AddFile(url, path, contentType, preLoad);

        /// <summary>
        /// Add file pointed by the path as a resource
        /// </summary>
        /// <param name="url">Path of the file in the server. Should start with a /</param>
        /// <param name="content">Content of the file to add</param>
        /// <param name="contentType">MIME type of the content to send</param>
        [Obsolete("Function will be removed in next version. Please use GetResourceManager instead")]
        public void AddFileContent(string url, string content, Http.MIME contentType) => m_ResourceManager.AddFileContent(url, content, contentType);

        /// <summary>
        /// Add a resource to the server
        /// </summary>
        /// <param name="url">Path of the resource in the server. Should start with a /</param>
        /// <param name="resource">The resource to add</param>
        /// <param name="allowBackTrack">Specify if the file allow URL backtracking (/a/b redirect to /a resource if backtrack is enable on /a and /a/b doesn't exist)</param>
        [Obsolete("Function will be removed in next version. Please use GetResourceManager instead")]
        public void AddResource(string url, Http.Resource resource, bool allowBackTrack = false) => m_ResourceManager.AddResource(url, resource, allowBackTrack);

        /// <summary>
        /// Add a controler to the server
        /// </summary>
        /// <param name="url">Path of the resource in the server. Should start with a /</param>
        /// <param name="controlerBuilder">Builder to create a new controler</param>
        [Obsolete("Function will be removed in next version. Please use GetResourceManager instead")]
        public void AddResource(string url, IControlerBuilder controlerBuilder) => m_ResourceManager.AddResource(url, controlerBuilder);

        /// <summary>
        /// Add a controler to the server
        /// </summary>
        /// <param name="url">Path of the resource in the server. Should start with a /</param>
        /// <param name="controlerBuilder">Delegate to create a new controler</param>
        /// <param name="singleInstance">If false, server will create one instance of the controller by client (false by default)</param>
        [Obsolete("Function will be removed in next version. Please use GetResourceManager instead")]
        public void AddResource(string url, DelegateControlerBuilder.Delegate controlerBuilder, bool singleInstance = false) => m_ResourceManager.AddResource(url, controlerBuilder, singleInstance);

        /// <summary>
        /// Start the webserver
        /// </summary>
        public void StartListening()
        {
            try
            {
                m_HttpServerSocket.Bind(new IPEndPoint(IPAddress.Any, m_Port));
                m_HttpServerSocket.Listen(10);
                m_HttpServerSocket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception ex)
            {
                Stop();
                throw new Exception("Listening error" + ex);
            }
        }

        /// <summary>
        /// Stop the webserver
        /// </summary>
        public void StopListening()
        {
            m_HttpServerSocket.Close();
        }

        /// <summary>
        /// Start the server and the update loop of the resource manager
        /// </summary>
        public void Start()
        {
            if (!m_Running)
            {
                m_Running = true;
                StartListening();
                m_ResourceManager.StartUpdateLoop();
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            m_ResourceManager.StopUpdateLoop();
            m_HttpServerSocket.Close();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket acceptedSocket = m_HttpServerSocket.EndAccept(ar);
                m_ClientManager.NewClient(this, acceptedSocket, m_ServerCertificate);
                m_HttpServerSocket.BeginAccept(AcceptCallback, m_HttpServerSocket);
            }
            catch
            {
                Stop();
            }
        }

        /// <summary>
        /// Function called when the client disconnect
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        public sealed override void OnClientDisconnect(int clientID) => m_ClientManager.Disconnect(clientID);

        /// <summary>
        /// Function called when the client receive an HTTP request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received</param>
        public sealed override void OnHttpRequest(int clientID, Http.Request request)
        {
            Http.Resource? resource = m_ResourceManager.GetResource(request.Path);
            if (resource != null && !(resource.IsSecured() && m_ServerCertificate == null))
            {
                if (m_MethodHandlers.TryGetValue(request.Method, out var handler) && handler(resource, clientID, request))
                    return;
                m_ClientManager.SendError(clientID, Http.Defines.NewResponse(405));
            }
            else
                m_ClientManager.SendError(clientID, Http.Defines.NewResponse(404));
        }

        /// <summary>
        /// Function called when the client receive a websocket message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="message">Websocket message received</param>
        public sealed override void OnWebSocketMessage(int clientID, string message)
        {
            if (m_ClientManager.TransferWebsocketMessage(clientID, message))
                return;
            else if (m_ResourceManager.GetControler(message, out var controler))
                m_ClientManager.LinkClientToControler(clientID, controler);
        }
    }
}
