using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

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

        private readonly Stopwatch m_Watch = new();
        private readonly Socket m_HttpServerSocket;
        private readonly int m_Port;
        private int m_CurrentIdx = 0;
        private readonly List<int> m_FreeIdx = new();
        private readonly Dictionary<int, Client> m_Clients = new();
        private readonly Dictionary<int, Controler> m_ClientsControlers = new();
        private readonly ResourceManager m_Resources = new();
        private readonly Dictionary<string, Controler> m_Controlers = new();
        private readonly Dictionary<string, MethodHandler> m_MethodHandlers = new();

        /// <summary>
        /// Create a Quicksand server on the specified <paramref name="port"/>
        /// </summary>
        /// <param name="port">Port on which the server will listen (80 by default)</param>
        public Server(int port = 80)
        {
            m_Port = port;
            m_MethodHandlers["GET"] = (Http.Resource resource, int clientID, Http.Request request) => { resource.CallGet(clientID, request); return true; };
            m_HttpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Iterate over directories pointed by the path to add all files as resources
        /// </summary>
        /// <example>
        /// File System:<br/>
        /// myTestDir<br/>
        /// |- dir1<br/>
        /// |   |- fileA.txt<br/>
        /// |   -- fileB.txt<br/>
        /// |- fileC.txt<br/>
        /// -- fileD.txt<br/>
        /// <c> server.AddResource("/test", "myTestDir", Http.MIME.TEXT.PLAIN); </c><br/>
        /// Will result in the add of resources:<br/>
        /// /test/dir1/testa.txt<br/>
        /// /test/dir1/testb.txt<br/>
        /// /test/testc.txt<br/>
        /// /test/testd.txt<br/>
        /// </example>
        /// <param name="url">Path of the file in the server. Should start with a /</param>
        /// <param name="path">Path of the file to add</param>
        /// <param name="contentType">MIME type of the content to send</param>
        /// <param name="preLoad">Specify if we should load in memory the file content (false by default)</param>
        [ObsoleteAttribute("Function will be removed in v0.0.9. Please use AddFile with MIME type instead of string")]
        public void AddResource(string url, string path, string contentType, bool preLoad = false)
        {
            if (File.Exists(path))
            {
                Http.MIME? contentMIME = Http.MIME.Parse(contentType);
                if (contentMIME == null)
                    return;
                AddResource(url, new Http.File(path, contentMIME, preLoad), false);
            }
            else if (Directory.Exists(path))
            {
                string[] entries = Directory.GetFileSystemEntries(path);
                foreach (string entry in entries)
                    AddResource(string.Format("{0}/{1}", (url.Length > 0 && url[^1] == '/') ? url[..^1] : url, Path.GetFileName(entry).Replace(' ', '_').ToLowerInvariant()), entry, contentType, preLoad);
            }
        }

        /// <summary>
        /// Add file pointed by the path as a resource
        /// </summary>
        /// <param name="url">Path of the file in the server. Should start with a /</param>
        /// <param name="path">Path of the file to add</param>
        /// <param name="contentType">MIME type of the content to send</param>
        /// <param name="preLoad">Specify if we should load in memory the file content (false by default)</param>
        public void AddFile(string url, string path, Http.MIME contentType, bool preLoad = false)
        {
            if (File.Exists(path))
                AddResource(url, new Http.File(path, contentType, preLoad), false);
        }

        /// <summary>
        /// Add a resource to the server
        /// </summary>
        /// <param name="url">Path of the resource in the server. Should start with a /</param>
        /// <param name="resource">The resource to add</param>
        /// <param name="allowBackTrack">Specify if the file allow URL backtracking (/a/b redirect to /a resource if backtrack is enable on /a and /a/b doesn't exist)</param>
        public void AddResource(string url, Http.Resource resource, bool allowBackTrack = false)
        {
            resource.Init(this);
            m_Resources.AddResource(url, resource, allowBackTrack);
        }

        /// <summary>
        /// Add a controler to the server
        /// </summary>
        /// <param name="url">Path of the resource in the server. Should start with a /</param>
        /// <param name="controlerBuilder">Builder to create a new controler</param>
        public void AddResource(string url, IControlerBuilder controlerBuilder)
        {
            AddResource(url, new Http.ControlerInstance(controlerBuilder));
        }

        /// <summary>
        /// Add a controler to the server
        /// </summary>
        /// <param name="url">Path of the resource in the server. Should start with a /</param>
        /// <param name="controlerBuilder">Delegate to create a new controler</param>
        /// <param name="singleInstance">If false, server will create one instance of the controller by client (false by default)</param>
        public void AddResource(string url, DelegateControlerBuilder.Delegate controlerBuilder, bool singleInstance = false)
        {
            AddResource(url, new Http.ControlerInstance(controlerBuilder, singleInstance));
        }

        internal void AddControler(Controler controler)
        {
            if (!m_Controlers.ContainsKey(controler.GetID()))
            {
                controler.Init(this);
                m_Controlers[controler.GetID()] = controler;
            }
        }

        /// <summary>
        /// Update the server
        /// </summary>
        public void Update()
        {
            m_Watch.Stop();
            List<string> controllerToRemove = new();
            foreach (Controler controler in m_Controlers.Values)
            {
                if (controler.NeedDelete())
                    controllerToRemove.Add(controler.GetID());
                else
                    controler.Update(m_Watch.ElapsedMilliseconds);
            }
            foreach (string toRemove in controllerToRemove)
                m_Controlers.Remove(toRemove);
            m_Watch.Restart();
        }

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
                m_Watch.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("listening error" + ex);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket acceptedSocket = m_HttpServerSocket.EndAccept(ar);
                int clientID = m_CurrentIdx;
                if (m_FreeIdx.Count == 0)
                    ++m_CurrentIdx;
                else
                {
                    clientID = m_FreeIdx[0];
                    m_FreeIdx.RemoveAt(0);
                }
                Client client = new(this, acceptedSocket, clientID);
                m_Clients[client.GetID()] = client;
                client.StartReceiving();
                m_HttpServerSocket.BeginAccept(AcceptCallback, m_HttpServerSocket);
            }
            catch (Exception ex)
            {
                throw new Exception("Base Accept error" + ex);
            }
        }

        /// <summary>
        /// Function called when the client disconnect
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        protected sealed override void OnClientDisconnect(int clientID)
        {
            if (m_ClientsControlers.TryGetValue(clientID, out var oldResource))
                oldResource.RemoveListener(clientID);
            m_Clients.Remove(clientID);
            m_FreeIdx.Add(clientID);
        }

        /// <summary>
        /// Function called when the client receive an HTTP request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received</param>
        protected sealed override void OnHttpRequest(int clientID, Http.Request request)
        {
            Http.Resource? resource = m_Resources.GetResource(request.Path);
            if (resource != null)
            {
                if (m_MethodHandlers.TryGetValue(request.Method, out var handler) && handler(resource, clientID, request))
                    return;
                SendError(clientID, Http.Defines.NewResponse(405));
            }
            else
                SendError(clientID, Http.Defines.NewResponse(404));
        }

        /// <summary>
        /// Send the given message to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the message</param>
        /// <param name="message">Message to send to the client</param>
        public void Send(int clientID, string message)
        {
            if (m_Clients.TryGetValue(clientID, out var client) && client.IsWebSocket())
                client.Send(message);
        }

        /// <summary>
        /// Send the given error response to the given client then close the connection with it
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="error">HTTP Response to send to the client</param>
        public void SendError(int clientID, Http.Response? error)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
            {
                client.SendResponse(error);
                client.Disconnect();
            }
        }

        /// <summary>
        /// Send the given HTTP response to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="response">HTTP response to send to the client</param>
        public void SendResponse(int clientID, Http.Response? response)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
                client.SendResponse(response);
        }

        /// <summary>
        /// Function called when the client receive a websocket message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="message">Websocket message received</param>
        protected sealed override void OnWebSocketMessage(int clientID, string message)
        {
            if (m_ClientsControlers.TryGetValue(clientID, out var resource))
                resource.WebsocketMessage(clientID, message);
            else if (m_Controlers.TryGetValue(message, out var controler))
            {
                if (m_Clients.ContainsKey(clientID))
                {
                    if (m_ClientsControlers.TryGetValue(clientID, out var oldControler))
                        oldControler.RemoveListener(clientID);
                    m_ClientsControlers[clientID] = controler;
                    controler.AddListener(clientID);
                }
            }
        }
    }
}
