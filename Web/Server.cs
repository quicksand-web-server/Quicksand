using System.Net;
using System.Net.Sockets;

namespace Quicksand.Web
{
    public class Server
    {
        private readonly Socket m_HttpServerSocket;
        private readonly int m_Port;
        private readonly IDGenerator m_IDGenerator = new();
        private readonly Dictionary<int, Client> m_Clients = new();
        private readonly Dictionary<int, Resource> m_ClientsResource = new();
        private readonly Dictionary<string, Resource> m_Resources = new();
        private readonly FileSystem m_FileSystem = new();

        public Server(int port = 80)
        {
            m_Port = port;
            m_HttpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void AddResource(string httpPath, string path, bool preLoad = false)
        {
            m_FileSystem.AddPath(httpPath, path, preLoad);
        }

        public void AddResource<TRes>(string url, params dynamic[] args) where TRes : Resource, new()
        {
            Resource newResource = new TRes();
            newResource.Init(this, url, args);
            m_Resources[url] = newResource;
        }

        public void Update(long deltaTime)
        {
            foreach (Resource resource in m_Resources.Values)
            {
                if (resource.NeedUpdate())
                    resource.Update(deltaTime);
            }
        }

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
                Client client = new(this, acceptedSocket, m_IDGenerator.NextIdx());
                m_Clients[client.GetID()] = client;
                client.StartReceiving();
                m_HttpServerSocket.BeginAccept(AcceptCallback, m_HttpServerSocket);
            }
            catch (Exception ex)
            {
                throw new Exception("Base Accept error" + ex);
            }
        }

        internal void RemoveUser(int clientID)
        {
            if (m_ClientsResource.TryGetValue(clientID, out var oldResource))
                oldResource.RemoveListener(clientID);
            m_Clients.Remove(clientID);
            m_IDGenerator.FreeIdx(clientID);
        }

        internal void TreatRequest(int clientID, Http.Request request)
        {
            if (m_Resources.TryGetValue(request.Path, out var resource))
            {

                if (request.HaveHeaderField("Sec-WebSocket-Key"))
                    ListenTo(clientID, resource);
                else
                    resource.OnRequest(clientID, request);
            }
            else
            {
                string? content = m_FileSystem.GetContent(request.Path);
                if (content != null)
                    SendResponse(clientID, Http.Defines.NewResponse(request.Version, 200, content));
                else
                    SendError(clientID, Http.Defines.NewResponse(request.Version, 404));
            }
        }

        public void Send(int clientID, string response)
        {
            if (m_Clients.TryGetValue(clientID, out var client) && client.IsWebSocket())
                client.Send(response);
        }

        public void SendError(int clientID, Http.Response? response)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
            {
                client.SendResponse(response);
                client.Disconnect();
            }
        }

        public void SendResponse(int clientID, Http.Response? response)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
                client.SendResponse(response);
        }

        internal void ListenTo(int clientID, Resource resource)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
            {
                if (m_ClientsResource.TryGetValue(clientID, out var oldResource))
                    oldResource.RemoveListener(clientID);
                m_ClientsResource[clientID] = resource;
                resource.AddListener(clientID);
            }
        }

        internal void OnWebsocketMessage(int clientID, string message)
        {
            if (m_ClientsResource.TryGetValue(clientID, out var resource))
                resource.OnWebsocketMessage(clientID, message);
        }
    }
}
