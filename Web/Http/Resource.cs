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
        protected void SendError(int clientID, Http.Response? error)
        {
            m_Server?.SendError(clientID, error);
        }

        /// <summary>
        /// Send the given HTTP response to the given client
        /// </summary>
        /// <param name="clientID">ID of the client to which the server should send the error</param>
        /// <param name="response">HTTP Response to send to the client</param>
        protected void SendResponse(int clientID, Http.Response? response)
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

        internal virtual void OnRequest(int clientID, Request request)
        {
            switch (request.Method)
            {
                case "GET":
                    {
                        Get(clientID, request);
                        break;
                    }
                case "HEAD":
                    {
                        Head(clientID, request);
                        break;
                    }
                case "POST":
                    {
                        Post(clientID, request);
                        break;
                    }
                case "PUT":
                    {
                        Put(clientID, request);
                        break;
                    }
                case "DELETE":
                    {
                        Delete(clientID, request);
                        break;
                    }
                case "CONNECT":
                    {
                        Connect(clientID, request);
                        break;
                    }
                case "OPTIONS":
                    {
                        Options(clientID, request);
                        break;
                    }
                case "TRACE":
                    {
                        Trace(clientID, request);
                        break;
                    }
                case "PATCH":
                    {
                        Patch(clientID, request);
                        break;
                    }
                default:
                    {
                        SendError(clientID, Http.Defines.NewResponse(400));
                        break;
                    }
            }
        }

        /// <summary>
        /// Function called when a GET is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected abstract void Get(int clientID, Http.Request request);

        /// <summary>
        /// Function called when a HEAD is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Head(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a POST is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Post(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a PUT is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Put(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a DELETE is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Delete(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a CONNECT is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Connect(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a OPTIONS is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Options(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a TRACE is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Trace(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a PATCH is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected virtual void Patch(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }
    }
}
