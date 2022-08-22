namespace Quicksand.Web.Http
{
    /// <summary>
    /// Class that represent a physical file resource
    /// </summary>
    public class File : Resource
    {
        private readonly string m_Path;
        private readonly string m_Content;
        private readonly string m_ContentType;

        private static System.Text.Encoding GetEncoding(string contentType)
        {
            if (contentType == "image/x-icon")
                return System.Text.Encoding.ASCII;
            return System.Text.Encoding.UTF8;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="preLoad">Specify if we should load in memory the file content (false by default)</param>
        public File(string path, string contentType, bool preLoad = false)
        {
            m_Path = path;
            m_ContentType = contentType;
            if (preLoad)
                m_Content = System.IO.File.ReadAllText(m_Path, GetEncoding(contentType));
            else
                m_Content = "";
        }

        /// <summary>
        /// Function called when a GET is requested on this file
        /// </summary>
        /// <remarks>
        /// When called it will send an HTTP 200 response with the file content. If file doesn't exist it will send a HTTP 404 error
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected sealed override void Get(int clientID, Request request)
        {
            if (m_Content.Length > 0)
                SendResponse(clientID, Defines.NewResponse(200, m_Content, m_ContentType));
            else if (System.IO.File.Exists(m_Path))
                SendResponse(clientID, Defines.NewResponse(200, System.IO.File.ReadAllText(m_Path, GetEncoding(m_ContentType)), m_ContentType));
            else
                SendError(clientID, Defines.NewResponse(404));
        }
    }
}
