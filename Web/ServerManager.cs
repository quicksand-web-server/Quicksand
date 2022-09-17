namespace Quicksand.Web
{
    /// <summary>
    /// Class to handle multiple instance of <seealso cref="Server"/>
    /// </summary>
    public class ServerManager
    {
        private readonly ClientManager m_ClientManager = new();
        private readonly ResourceManager m_ResourceManager;
        private readonly Dictionary<int, Server> m_Servers = new();
        private volatile bool m_Running = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public ServerManager() => m_ResourceManager = new(m_ClientManager);

        /// <summary>
        /// Create a Quicksand HTTP server on the specified <paramref name="port"/>
        /// </summary>
        /// <param name="port">Port on which the server will listen (80 by default)</param>
        public void NewServer(int port = 80)
        {
            if (!m_Servers.ContainsKey(port))
            {
                Server server = new(m_ClientManager, m_ResourceManager, port);
                if (m_Running)
                    server.StartListening();
                m_Servers.Add(port, server);
            }
        }

        /// <summary>
        /// Create a Quicksand HTTPS server on the specified <paramref name="port"/>
        /// </summary>
        /// <param name="path">Path of the SSL certificate file to use on the server</param>
        /// <param name="port">Port on which the server will listen (443 by default)</param>
        public void NewSecuredServer(string path, int port = 443)
        {
            if (!m_Servers.ContainsKey(port))
            {
                Server server = new(m_ClientManager, m_ResourceManager, port);
                server.LoadCertificate(path);
                if (m_Running)
                    server.StartListening();
                m_Servers.Add(port, server);
            }
        }

        /// <returns>The <seealso cref="ResourceManager"/> of the all the managed servers</returns>
        public ResourceManager GetResourceManager() => m_ResourceManager;

        /// <summary>
        /// Start all the managed servers and the update loop of the resource manager
        /// </summary>
        public void Start()
        {
            if (!m_Running)
            {
                m_Running = true;
                foreach (Server server in m_Servers.Values)
                    server.StartListening();
                m_ResourceManager.StartUpdateLoop();
            }
        }
    }
}
