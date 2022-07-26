using Quicksand.Web.Http;

namespace Quicksand.Web.Resources
{
    /// <summary>
    /// Class that represent a physical file resource
    /// </summary>
    /// <remarks>
    /// To use this class in <seealso cref="Server.AddResource&lt;TRes&gt;"/> please create a child class first and use child class
    /// This class is instantiated by <seealso cref="Server.AddResource"/>
    /// </remarks>
    public class File : Resource
    {
        private readonly string m_Path;
        private readonly string m_Content;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="preLoad">Specify if we should load in memory the file content. False by default</param>
        public File(string path, bool preLoad = false)
        {
            m_Path = path;
            if (preLoad)
                m_Content = System.IO.File.ReadAllText(m_Path);
            else
                m_Content = "";
        }

        /// <summary>
        /// UNUSED
        /// </summary>
        protected sealed override void OnInit(params dynamic[] _) {}

        /// <summary>
        /// Function called when a GET is requested on this file
        /// </summary>
        /// <remarks>
        /// When called it will send an HTTP 200 response with the file content. If file doesn't exist it will send a HTTP 404 error
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected sealed override void Get(int clientID, Request request)
        {
            if (m_Content.Length > 0)
                SendResponse(clientID, Defines.NewResponse(200, m_Content));
            else if (System.IO.File.Exists(m_Path))
                SendResponse(clientID, Defines.NewResponse(200, System.IO.File.ReadAllText(m_Path)));
            else
                SendError(clientID, Defines.NewResponse(404));
        }
    }
}
