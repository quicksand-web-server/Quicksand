namespace Quicksand.Web
{
    /// <summary>
    /// Represent a http resource targetable by request
    /// </summary>
    public abstract class Resource
    {
        private readonly bool m_NeedUpdate = false;
        private Server? m_Server = null;
        private readonly List<int> m_Listeners = new();

        /// <summary>
        /// Constructor
        /// </summary>
        protected Resource() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="needUpdate">Specify if this resource need to be updated or not</param>
        protected Resource(bool needUpdate)
        {
            m_NeedUpdate = needUpdate;
        }

        internal bool NeedUpdate() { return m_NeedUpdate; }

        internal void Init(Server server, params dynamic[] args)
        {
            m_Server = server;
            OnInit(args);
        }

        internal void Update(long deltaTime)
        {
            OnUpdate(deltaTime);
        }

        /// <summary>
        /// Function called when initializing the resources
        /// </summary>
        /// <param name="args">Arguments of the initialization</param>
        protected abstract void OnInit(params dynamic[] args);

        /// <summary>
        /// Function called when updating the resource
        /// </summary>
        /// <param name="deltaTime">Delta time in milliseconds since last update</param>
        protected virtual void OnUpdate(long deltaTime) {}


        /// <summary>
        /// Send the given error response to all the clients listening to this resource then close the connection with them
        /// </summary>
        /// <param name="error">HTTP Response to send to the client</param>
        protected void SendError(Http.Response? error)
        {
            if (m_Server != null)
            {
                foreach (int clientID in m_Listeners)
                    m_Server.SendError(clientID, error);
            }
        }

        /// <summary>
        /// Send the given message to all the clients listening to this resource
        /// </summary>
        /// <param name="message">Message to send to the client</param>
        protected void Send(string message)
        {
            if (m_Server != null)
            {
                foreach (int clientID in m_Listeners)
                    m_Server.Send(clientID, message);
            }
        }


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
        /// Send the given http response to the given client
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

        internal void AddListener(int listenerID)
        {
            m_Listeners.Add(listenerID);
        }

        internal void RemoveListener(int listenerID)
        {
            m_Listeners.Remove(listenerID);
        }

        internal virtual void OnRequest(int clientID, Http.Request request)
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

        internal void OnWebsocketMessage(int clientID, string message)
        {
            WebsocketMessage(clientID, message);
        }

        /// <summary>
        /// Function called when the resource receive a websocket message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="message">Received message from the websocket</param>
        protected virtual void WebsocketMessage(int clientID, string message) { }

        /// <summary>
        /// Function called when a GET is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected abstract void Get(int clientID, Http.Request request);

        /// <summary>
        /// Function called when a HEAD is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Head(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a POST is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Post(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a PUT is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Put(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a DELETE is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Delete(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a CONNECT is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Connect(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a OPTIONS is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Options(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a TRACE is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Trace(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }

        /// <summary>
        /// Function called when a PATCH is requested on this resource
        /// </summary>
        /// <remarks>
        /// Send an HTTP 405 by default
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected virtual void Patch(int clientID, Http.Request request) { SendError(clientID, Http.Defines.NewResponse(405)); }
    }
}
