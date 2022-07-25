namespace Quicksand.Web
{
    public class Resource
    {
        private readonly bool m_NeedUpdate = false;
        private Server? m_Server = null;
        private string m_URL = "";
        private readonly List<int> m_Listeners = new();

        public Resource() { }
        protected Resource(bool needUpdate)
        {
            m_NeedUpdate = needUpdate;
        }

        internal bool NeedUpdate() { return m_NeedUpdate; }

        internal void Init(Server server, string url, params dynamic[] args)
        {
            m_URL = url;
            m_Server = server;
            OnInit(args);
        }

        internal void Update(long deltaTime)
        {
            OnUpdate(deltaTime);
        }

        protected virtual void OnInit(params dynamic[] args) { }

        protected virtual void OnUpdate(long deltaTime) {}

        public string URL { get { return m_URL; } }

        protected void SendError(Http.Response? response)
        {
            if (m_Server != null)
            {
                foreach (int listenerID in m_Listeners)
                    m_Server.SendError(listenerID, response);
            }
        }

        protected void Send(string response)
        {
            if (m_Server != null)
            {
                foreach (int listenerID in m_Listeners)
                    m_Server.Send(listenerID, response);
            }
        }

        protected void SendError(int userID, Http.Response? response)
        {
            m_Server?.SendError(userID, response);
        }

        protected void SendResponse(int userID, Http.Response? response)
        {
            m_Server?.SendResponse(userID, response);
        }

        protected void Send(int userID, string response)
        {
            m_Server?.Send(userID, response);
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
            }
        }

        internal void OnWebsocketMessage(int clientID, string message)
        {
            WebsocketMessage(clientID, message);
        }

        protected virtual void WebsocketMessage(int clientID, string message) { }

        private void NotImplementedYet(int clientID, string version) { SendError(clientID, Http.Defines.NewResponse(version, 501)); }

        protected virtual void Get(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Head(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Post(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Put(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Delete(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Connect(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Options(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Trace(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
        protected virtual void Patch(int clientID, Http.Request request) { NotImplementedYet(clientID, request.Version); }
    }
}
