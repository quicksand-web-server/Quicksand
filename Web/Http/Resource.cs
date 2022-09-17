namespace Quicksand.Web.Http
{
    /// <summary>
    /// Represent a HTTP resource targetable by request
    /// </summary>
    public abstract class Resource
    {
        private bool m_IsSecured = false;
        private ResourceManager? m_ResourceManager = null;
        private ClientManager? m_ClientManager = null;

        internal void Init(ResourceManager resourceManager, ClientManager clientManager)
        {
            m_ResourceManager = resourceManager;
            m_ClientManager = clientManager;
        }

        internal void AddControler(Controler controler) => m_ResourceManager?.AddControler(controler);

        internal bool IsSecured() => m_IsSecured;

        /// <param name="clientID">ID of the client to check</param>
        /// <returns>If the client is a secured connection or not</returns>
        protected bool IsSecured(int clientID)
        {
            if (m_ClientManager != null)
                return m_ClientManager.IsSecured(clientID);
            return false;
        }

        /// <summary>
        /// Send the given error response to the given client then close the connection with it
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="error">HTTP Response to send to the client</param>
        protected void SendError(int clientID, Response? error) => m_ClientManager?.SendError(clientID, error);

        /// <summary>
        /// Send the given HTTP response to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="response">HTTP Response to send to the client</param>
        protected void SendResponse(int clientID, Response? response) => m_ClientManager?.SendResponse(clientID, response);

        /// <summary>
        /// Send the given message to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the message</param>
        /// <param name="message">Message to send to the client</param>
        protected void Send(int clientID, string message) => m_ClientManager?.Send(clientID, message);

        internal void CallGet(int clientID, Request request) => Get(clientID, request);

        /// <summary>
        /// Function called when a GET is requested on this resource
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received from the client</param>
        protected abstract void Get(int clientID, Request request);
    }
}
