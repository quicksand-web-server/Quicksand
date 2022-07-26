namespace Quicksand.Web.Resources
{
    /// <summary>
    /// Code driven HTML page
    /// </summary>
    public abstract class CodeDrivenHtml : Resource
    {
        /// <summary>
        /// Html document to send
        /// </summary>
        protected Html.Document m_Page = new("");

        /// <summary>
        /// Constructor
        /// </summary>
        public CodeDrivenHtml() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="needUpdate">Specify if this resource need to be updated or not</param>
        protected CodeDrivenHtml(bool needUpdate) : base(needUpdate) { }

        /// <summary>
        /// Initialization function
        /// </summary>
        /// <param name="args">args[0]: string Title of the HTML page to generate</param>
        protected override void OnInit(params dynamic[] args)
        {
            if (args.Length >= 1)
            {
                m_Page = new(args[0]);
                Generate();
            }
        }

        /// <summary>
        /// Function called when a GET is requested on this file
        /// </summary>
        /// <remarks>
        /// When called it will send an HTTP 200 response with the generated html content. If document hasn't been initialized it will send a HTTP 404 error
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received Http request received from the client</param>
        protected override void Get(int clientID, Http.Request request)
        {
            if (m_Page != null)
                SendResponse(clientID, Http.Defines.NewResponse(200, m_Page.ToString()));
            else
                SendResponse(clientID, Http.Defines.NewResponse(404));
        }

        /// <summary>
        /// Function called to generate the HTML page
        /// </summary>
        protected abstract void Generate();
    }
}
