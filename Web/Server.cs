using System.Net;
using System.Net.Sockets;

namespace Quicksand.Web
{
    /// <summary>
    /// A webserver that can also handle websocket connection
    /// </summary>
    public class Server: AClientListener
    {
        private readonly Socket m_HttpServerSocket;
        private readonly int m_Port;
        private int m_CurrentIdx = 0;
        private readonly List<int> m_FreeIdx = new();
        private readonly Dictionary<int, Client> m_Clients = new();
        private readonly Dictionary<int, Resource> m_ClientsResource = new();
        private readonly Dictionary<string, Resource> m_Resources = new();

        /// <summary>
        /// Create a Quicksand server on the specified <paramref name="port"/>
        /// </summary>
        /// <param name="port">Port on which the server will listen. 80 by default</param>
        public Server(int port = 80)
        {
            m_Port = port;
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
        /// <c> server.AddResource("/test", "myTestDir"); </c><br/>
        /// Will result in the add of resources:<br/>
        /// /test/dir1/testa.txt<br/>
        /// /test/dir1/testb.txt<br/>
        /// /test/testc.txt<br/>
        /// /test/testd.txt<br/>
        /// </example>
        /// <param name="url">Path of the file in the server. Should start with a /</param>
        /// <param name="path">Path of the file to add</param>
        /// <param name="preLoad">Specify if we should load in memory the file content. False by default</param>
        public void AddResource(string url, string path, bool preLoad = false)
        {
            if (File.Exists(path))
                m_Resources[url] = new Resources.File(path, preLoad);
            else if (Directory.Exists(path))
            {
                string[] entries = Directory.GetFileSystemEntries(path);
                foreach (string entry in entries)
                    AddResource(string.Format("{0}/{1}", (url.Length > 0 && url[^1] == '/') ? url[..^1] : url, Path.GetFileName(entry).Replace(' ', '_').ToLowerInvariant()), entry, preLoad);
            }
        }

        /// <summary>
        /// Add a resource to the server
        /// </summary>
        /// <param name="url">Path of the resource in the server. Should start with a /</param>
        /// <param name="args">Parameters to send to the OnInit method of the resource</param>
        /// <typeparam name="TRes">The type of resource to instanciate</typeparam>
        public void AddResource<TRes>(string url, params dynamic[] args) where TRes : Resource, new()
        {
            Resource newResource = new TRes();
            newResource.Init(this, args);
            m_Resources[url] = newResource;
        }

        /// <summary>
        /// Update the server
        /// </summary>
        /// <param name="deltaTime">Elapsed time in milliseconds since last update</param>
        public void Update(long deltaTime)
        {
            foreach (Resource resource in m_Resources.Values)
            {
                if (resource.NeedUpdate())
                    resource.Update(deltaTime);
            }
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
                Console.WriteLine($"Accept CallBack port:{m_Port} protocol type: {ProtocolType.Tcp}");
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
            if (m_ClientsResource.TryGetValue(clientID, out var oldResource))
                oldResource.RemoveListener(clientID);
            m_Clients.Remove(clientID);
            m_FreeIdx.Add(clientID);
        }

        /// <summary>
        /// Function called when the client receive an http request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Http request received</param>
        protected sealed override void OnHttpRequest(int clientID, Http.Request request)
        {
            if (m_Resources.TryGetValue(request.Path, out var resource))
            {

                if (request.HaveHeaderField("Sec-WebSocket-Key"))
                    ListenTo(clientID, resource);
                else
                    resource.OnRequest(clientID, request);
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
        /// Send the given http response to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="response">HTTP response to send to the client</param>
        public void SendResponse(int clientID, Http.Response? response)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
                client.SendResponse(response);
        }

        internal void ListenTo(int clientID, Resource resource)
        {
            if (m_Clients.ContainsKey(clientID))
            {
                if (m_ClientsResource.TryGetValue(clientID, out var oldResource))
                    oldResource.RemoveListener(clientID);
                m_ClientsResource[clientID] = resource;
                resource.AddListener(clientID);
            }
        }

        /// <summary>
        /// Function called when the client receive a websocket message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="message">Websocket message received</param>
        protected sealed override void OnWebSocketMessage(int clientID, string message)
        {
            if (m_ClientsResource.TryGetValue(clientID, out var resource))
                resource.OnWebsocketMessage(clientID, message);
        }
    }
}
