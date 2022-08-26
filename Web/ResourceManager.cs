namespace Quicksand.Web
{
    internal class ResourceManager
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

        private readonly ResourceManagerNode m_Root = new();

        private List<string> SplitURL(string url)
        {
            if (url[0] == '/')
                url = url[1..];
            List<string> path = url.Split('/').ToList();
            path.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            return path;
        }

        public void RemoveResource(string url)
        {
            m_Root.RemoveResource(SplitURL(url));
        }

        public void AddResource(string url, Http.Resource resource, bool allowBackTrack = false)
        {
            m_Root.SetResource(SplitURL(url), resource, allowBackTrack);
        }
        
        public Http.Resource? GetResource(string url)
        {
            return m_Root.GetResource(SplitURL(url));
        }
    }
}
