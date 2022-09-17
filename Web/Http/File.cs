namespace Quicksand.Web.Http
{
    /// <summary>
    /// Class that represent a physical file resource
    /// </summary>
    public class File : Resource
    {
        private readonly string m_Path;
        private readonly string m_Content;
        private readonly MIME m_ContentType;

        private static System.Text.Encoding GetEncoding(MIME contentType)
        {
            return contentType.GetMIMEType() switch
            {
                "image" => contentType.GetSubType() switch
                {
                    "x-icon" => System.Text.Encoding.ASCII,
                    _ => System.Text.Encoding.UTF8
                },
                _ => System.Text.Encoding.UTF8
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="preLoad">Specify if we should load in memory the file content</param>
        public File(string path, MIME contentType, bool preLoad): base()
        {
            m_Path = path;
            m_ContentType = contentType;
            if (preLoad)
                m_Content = System.IO.File.ReadAllText(m_Path, GetEncoding(contentType));
            else
                m_Content = "";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Content of the file</param>
        /// <param name="contentType">MIME type of the file</param>
        public File(string content, MIME contentType) : base()
        {
            m_Path = "";
            m_ContentType = contentType;
            m_Content = content;
        }

        /// <summary>
        /// Function called when a GET is requested on this file
        /// </summary>
        /// <remarks>
        /// When called it will send an HTTP 200 response with the file content. If file doesn't exist it will send a HTTP 404 error
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received from the client</param>
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
