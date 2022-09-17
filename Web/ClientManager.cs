using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Quicksand.Web
{
    internal class ClientManager
    {
        private int m_CurrentIdx = 0;
        private readonly List<int> m_FreeIdx = new();
        private readonly Dictionary<int, Client> m_Clients = new();
        private readonly Dictionary<int, Controler> m_ClientsControler = new();

        internal void NewClient(AClientListener listener, Socket socket, X509Certificate? certificate = null)
        {
            int clientID = m_CurrentIdx;
            if (m_FreeIdx.Count == 0)
                ++m_CurrentIdx;
            else
            {
                clientID = m_FreeIdx[0];
                m_FreeIdx.RemoveAt(0);
            }
            Client client = new(listener, socket, clientID, certificate);
            m_Clients[client.GetID()] = client;
            client.StartReceiving();
        }

        internal void Disconnect(int id)
        {
            if (m_ClientsControler.TryGetValue(id, out var oldResource))
                oldResource.RemoveListener(id);
            m_Clients.Remove(id);
            m_FreeIdx.Add(id);
        }

        internal void LinkClientToControler(int id, Controler controler)
        {
            if (m_Clients.ContainsKey(id))
            {
                if (m_ClientsControler.TryGetValue(id, out var oldControler))
                    oldControler.RemoveListener(id);
                m_ClientsControler[id] = controler;
                controler.AddListener(id);
            }
        }

        internal bool TransferWebsocketMessage(int id, string message)
        {
            if (m_ClientsControler.TryGetValue(id, out var resource))
            {
                resource.WebsocketMessage(id, message);
                return true;
            }
            return false;
        }

        public void Send(int clientID, string message)
        {
            if (m_Clients.TryGetValue(clientID, out var client) && client.IsWebSocket())
                client.Send(message);
        }

        public void SendError(int clientID, Http.Response? error)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
            {
                client.SendResponse(error);
                client.Disconnect();
            }
        }

        public void SendResponse(int clientID, Http.Response? response)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
                client.SendResponse(response);
        }

        public bool IsSecured(int clientID)
        {
            if (m_Clients.TryGetValue(clientID, out var client))
                return client.IsSecured();
            return false;
        }
    }
}
