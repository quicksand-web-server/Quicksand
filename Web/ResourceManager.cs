using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace Quicksand.Web
{
    /// <summary>
    /// Class use by <seealso cref="Server"/> to manage all it's resources
    /// </summary>
    public class ResourceManager
    {
        internal class ResourceManagerNode
        {
            private bool m_AllowBackTrack = false;
            private Http.Resource? m_Resource = null;
            private readonly Dictionary<string, ResourceManagerNode> m_Nodes = new();

            public void RemoveResource(List<string> path)
            {
                if (path.Count == 0)
                {
                    m_AllowBackTrack = false;
                    m_Resource = null;
                    return;
                }

                string nodeName = path[0];
                path.RemoveAt(0);
                if (!m_Nodes.ContainsKey(nodeName))
                    m_Nodes[nodeName] = new();
                m_Nodes[nodeName].RemoveResource(path);
            }

            public void SetResource(List<string> path, Http.Resource resource, bool allowBackTrack)
            {
                if (path.Count == 0)
                {
                    m_AllowBackTrack = allowBackTrack;
                    m_Resource = resource;
                    return;
                }

                string nodeName = path[0];
                path.RemoveAt(0);
                if (!m_Nodes.ContainsKey(nodeName))
                    m_Nodes[nodeName] = new();
                m_Nodes[nodeName].SetResource(path, resource, allowBackTrack);
            }

            public Http.Resource? GetResource(List<string> path)
            {
                if (path.Count == 0)
                    return m_Resource;
                else if (m_Nodes.TryGetValue(path[0], out ResourceManagerNode? node))
                {
                    path.RemoveAt(0);
                    return node.GetResource(path);
                }
                else if (m_AllowBackTrack)
                    return m_Resource;
                return null;
            }
        }

        private readonly Stopwatch m_Watch = new();
        private readonly ResourceManagerNode m_Root = new();
        private readonly Dictionary<string, Controler> m_Controlers = new();
        private readonly ClientManager m_ClientManager;
        private readonly Timer[] m_Timers = new Timer[1];
        private volatile bool m_Running = false;

        internal ResourceManager(ClientManager clientManager) => m_ClientManager = clientManager;

        private List<string> SplitURL(string url)
        {
            if (!string.IsNullOrWhiteSpace(url) && url[0] == '/')
                url = url[1..];
            List<string> path = url.Split('/').ToList();
            path.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            return path;
        }

        /// <summary>
        /// Remove the resource at the given url
        /// </summary>
        /// <param name="url">Path of the resource in the manager. Should start with a /</param>
        public void RemoveResource(string url) => m_Root.RemoveResource(SplitURL(url));

        /// <summary>
        /// Add file pointed by the path as a resource
        /// </summary>
        /// <param name="url">Path of the file in the manager. Should start with a /</param>
        /// <param name="path">Path of the file to add</param>
        /// <param name="contentType">MIME type of the content to send</param>
        /// <param name="preLoad">Specify if we should load in memory the file content (false by default)</param>
        public void AddFile(string url, string path, Http.MIME contentType, bool preLoad = false)
        {
            if (File.Exists(path))
                AddResource(url, new Http.File(path, contentType, preLoad), false);
        }

        /// <summary>
        /// Add file pointed by the path as a resource
        /// </summary>
        /// <param name="url">Path of the file in the manager. Should start with a /</param>
        /// <param name="content">Content of the file to add</param>
        /// <param name="contentType">MIME type of the content to send</param>
        public void AddFileContent(string url, string content, Http.MIME contentType) => AddResource(url, new Http.File(content, contentType), false);

        /// <summary>
        /// Add a resource to the manager
        /// </summary>
        /// <param name="url">Path of the resource in the manager. Should start with a /</param>
        /// <param name="resource">The resource to add</param>
        /// <param name="allowBackTrack">Specify if the file allow URL backtracking (/a/b redirect to /a resource if backtrack is enable on /a and /a/b doesn't exist)</param>
        public void AddResource(string url, Http.Resource resource, bool allowBackTrack = false)
        {
            resource.Init(this, m_ClientManager);
            m_Root.SetResource(SplitURL(url), resource, allowBackTrack);
        }

        /// <summary>
        /// Add a controler to the manager
        /// </summary>
        /// <param name="url">Path of the resource in the manager. Should start with a /</param>
        /// <param name="controlerBuilder">Builder to create a new controler</param>
        public void AddResource(string url, IControlerBuilder controlerBuilder) => AddResource(url, new Http.ControlerInstance(controlerBuilder));

        /// <summary>
        /// Add a controler to the manager
        /// </summary>
        /// <param name="url">Path of the resource in the manager. Should start with a /</param>
        /// <param name="controlerBuilder">Delegate to create a new controler</param>
        /// <param name="singleInstance">If false, manager will create one instance of the controller by client (false by default)</param>
        public void AddResource(string url, DelegateControlerBuilder.Delegate controlerBuilder, bool singleInstance = false) => AddResource(url, new Http.ControlerInstance(controlerBuilder, singleInstance));

        /// <summary>
        /// Add Quicksand javascript framework to the resources
        /// </summary>
        public void AddFramework() => AddResource("/framework.js", new Framework());

        /// <param name="url">Path of the resource in the manager. Should start with a /</param>
        /// <returns>The resource at the given path (null if no resource)</returns>
        public Http.Resource? GetResource(string url) => m_Root.GetResource(SplitURL(url));

        internal bool GetControler(string id, out Controler ret)
        {
            if (m_Controlers.TryGetValue(id, out var controler))
            {
                ret = controler;
                return true;
            }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            ret = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return false;
        }

        internal void AddControler(Controler controler)
        {
            if (!m_Controlers.ContainsKey(controler.GetID()))
            {
                controler.Init(this, m_ClientManager);
                m_Controlers[controler.GetID()] = controler;
            }
        }

        /// <summary>
        /// Start an update loop that will update the manager regularly
        /// </summary>
        /// <param name="interval">Interval in milliseconds to refresh the manager (50 by default)</param>
        public void StartUpdateLoop(double interval = 50)
        {
            if (!m_Running)
            {
                m_Running = true;
                m_Timers[0] = new(interval);
                m_Timers[0].Elapsed += (sender, e) => Update();
                m_Timers[0].Start();
            }
        }

        /// <summary>
        /// Stop an update loop that will update the manager regularly
        /// </summary>
        public void StopUpdateLoop()
        {
            if (m_Running)
            {
                m_Running = false;
                m_Timers[0].Stop();
            }
        }

        /// <summary>
        /// Update the manager
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
    }
}
