namespace Quicksand.Web.Http
{
    /// <summary>
    /// Represent a HTTP resource targetable by request
    /// </summary>
    public abstract class Resource
    {
        private Server? m_Server = null;

        internal void Init(Server server)
        {
            m_Server = server;
        }

        internal void AddControler(Controler controler) { m_Server?.AddControler(controler); }

        /// <summary>
        /// Send the given error response to the given client then close the connection with it
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="error">HTTP Response to send to the client</param>
        protected void SendError(int clientID, Response? error)
        {
            m_Server?.SendError(clientID, error);
        }

        /// <summary>
        /// Send the given HTTP response to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="response">HTTP Response to send to the client</param>
        protected void SendResponse(int clientID, Response? response)
        {
            m_Server?.SendResponse(clientID, response);
        }

        /// <summary>
        /// Send the given message to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the message</param>
        /// <param name="message">Message to send to the client</param>
        protected void Send(int clientID, string message)
        {
            m_Server?.Send(clientID, message);
        }

        internal void CallGet(int clientID, Request request)
        {
            Get(clientID, request);
        }

        /// <summary>
        /// Function called when a GET is requested on this resource
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received from the client</param>
        protected abstract void Get(int clientID, Request request);
    }
}
