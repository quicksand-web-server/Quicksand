using Quicksand.Web.Html;
using System.Diagnostics;

namespace Quicksand.Web
{
    /// <summary>
    /// Controler for a <seealso cref="Model"/>
    /// </summary>
    public abstract class Controler : Http.Resource
    {
        internal static readonly string FAVICON = "data:image/x-icon;base64,AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAABMLAAATCwAAAAAAAAAAAAD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8AIGDl/x9Z5P8fU+L/////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8AIWfl/yBi5v8fXOT/H1bj/x9P4f8fSeD/HkLf/////wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////ACOk8/8jnfP/Ipfy/yKR8P////8A////AP///wD///8A////AP///wAgZeb/IF/l/x9Y4/8fUuL/H0zh/x5F3/8eP97/HTnd/////wD///8A////AP///wAcGdX/////AP///wD///8A////AP///wD///8A////AP///wD///8AI6D0/yKa8v8ik/H/Io3v/yKH7v8hgOz/////AP///wD///8AIGfn/yBh5f8fW+T/H1Xj/x9P4v8fSOD/HkLf/x483f8eNdz/HS/a/x0p2f8dItf/HBzW/xwW1f8cENP/GwjO/////wD///8A////AP///wD///8A////AP///wAjnPP/Ipbx/yKQ8P8iiu//IYPt/yF97P8hd+v/////AP///wD///8AIF7l/x9X4/8fUeL/H0vg/////wAeP97/Hjjc/x0y2/8dK9n/HSXY/x0f1/8cGdX/HBLU/xwM0v8bBtH/////AP///wD///8A////AP///wD///8A////ACKZ8v8ikvH/Iozv/yKG7v8hgOz/IXnr/yFz6v8gbej/////AP///wAgWuX/H1Ti/x9N4f8eR+D/H0He/////wAdNdv/HS7a/x0o2f8cItb/HBvW/xwV1P8cD9P/GwjR/////wD///8A////AP///wD///8A////AP///wD///8AH5fz/yKP8P8iie7/IYLt/yF87P8hdur/IG/p/yBp5/8gY+b/////AP///wAfUOL/H0rg/x5E3/8ePd3/Hjba/////wD///8AHSTY/xwe1v8cGNX/HBHU/xwL0v////8A////AP///wD///8A////AP///wD///8A////AP///wD///8AIovu/yGF7v8fiO7/////ACBy6f8gbOj/IGbm/yBf5f8fWeT/////AB9M4f8eRt//HkDe/x463f8dM9v/////AP///wD///8A////ABwU1P8cDtL/////AP///wD///8A////AP///wD///8A////ACOh9P8imvL/IpTy/////wD///8AIYHt/yF76/8hdOn/////ACBo5/8gYub/H1zk/x9V4/8fT+H/////AB5D3/8ePN3/Hjbc/x0w2v8dKdn/////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wAjo/X/I53z/yKX8v8ikfD/Iorv/////wD///8AIXfr/yBx6f8hbOj/////ACBe5f8fWOP/H1Li/x9M4f8eRd//////AB453P8dMtv/HSza/x0m2P8bHtn/////AP///wD///8A////AP///wD///8A////AP///wD///8AJKP1/yOg9P////8A////AP///wAhh+7/IYDt/yF66v8fc+n/IG7o/yBn5/////8A////AB9U4/8fTuH/H0jg/x5C3v8eO93/HTTd/x0v2v8dKNn/HSLX/xwc1v////8A////AP///wD///8A////AP///wD///8A////AP///wD///8AIpzz/////wD///8A////AP///wD///8AIXbq/yBw6f8gauj/IGTm/yBd5f////8A////AB5J4f8eRN//Hj7e/x443P8dMdv/HSvZ/x0l2P8cH9f/HBjV/xwT0/////8A////AP///wD///8A////AP///wD///8A////AP///wD///8AIpLx/////wD///8A////AP///wD///8AIG3o/yBm5/8gYOX/H1rk/yBU4/////8A////AB5B3v8eOt3/HjTb/x0u2v8dKNn/HSHX/xwb1v8cFdT/HA/T/////wD///8A////AP///wD///8A////AP///wD///8A////AP///wAij/D/Iojw/////wD///8A////AP///wD///8AIGPm/yBc5P8fVuP/H1Di/////wD///8A////AP///wAdMNv/HSrZ/x0k2P8cHtb/HBfV/xwR0/////8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8AH1nj/x9T4v8fTOH/////AP///wD///8A////AP///wAdJ9n/HSDX/xwa1f8cFNT/Gw7T/////wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8AHBfV/xsR0/////8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A/////////////////////////////////////////////8f///8B//w/APf8DgAB/AcIAfwDBAP8AYMH/iCDz+MQQf/BiCD/nAwA/98GAH/vgwB/58PA///j4P////n///////////////////////////////////////////8=";

        /// <summary>
        /// HTML document to send
        /// </summary>
        protected Model m_Model = new("");
        private readonly string m_ID = Guid.NewGuid().ToString();
        private readonly List<int> m_Listeners = new();
        private readonly Stopwatch m_DeleteWatch = new();
        private bool m_CanBeDeleted = true;
        private bool m_ToDelete = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The model to use</param>
        protected Controler(Model? model)
        {
            if (model == null)
                m_Model = new("");
            else
            {
                m_Model = model;
                Script framework = new();
                framework.SetSrc("/framework.js"); //TODO Use a fixed URL
                m_Model.GetHead().AddScript(framework);
                m_Model.GetBody().SetAttribute("onload", string.Format("QuickSandFramework.main('{0}')", m_ID));
                AfterGenerateModel();
            }
            m_DeleteWatch.Start();
        }

        internal void MarkAsPermanent()
        {
            m_CanBeDeleted = false;
        }

        internal bool NeedDelete()
        {
            if (m_ToDelete)
            {
                long elapsedTime = m_DeleteWatch.ElapsedMilliseconds;
                return (elapsedTime >= 600000); //Return true if the controler is mark for delete since 10 minutes
            }
            else
                return false;
        }

        internal string GetID() { return m_ID; }

        /// <summary>
        /// Send the given message to all the clients listening to this resource
        /// </summary>
        /// <param name="message">Message to send to the client</param>
        protected void Send(string message)
        {
            foreach (int clientID in m_Listeners)
                Send(clientID, message);
        }

        internal void AddListener(int listenerID)
        {
            m_Listeners.Add(listenerID);
            m_ToDelete = false;
            m_DeleteWatch.Stop();
        }

        internal void RemoveListener(int listenerID)
        {
            m_Listeners.Remove(listenerID);
            if (m_CanBeDeleted)
            {
                m_ToDelete = m_Listeners.Count == 0;
                m_DeleteWatch.Restart();
            }
        }

        internal void WebsocketMessage(int clientID, string message)
        {
            OnWebsocketMessage(clientID, message);
        }

        /// <summary>
        /// Function called when the resource receive a websocket message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="message">Received message from the websocket</param>
        protected virtual void OnWebsocketMessage(int clientID, string message) {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the HTML code to send</param>
        protected Controler(string title)
        {
            m_Model = new(title);
            Generate();
        }

        /// <summary>
        /// Function called when a GET is requested on this file
        /// </summary>
        /// <remarks>
        /// When called it will send an HTTP 200 response with the generated HTML content. If document hasn't been initialized it will send a HTTP 404 error
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Received HTTP request received from the client</param>
        protected override void Get(int clientID, Http.Request request)
        {
            if (m_Model != null)
                SendResponse(clientID, Http.Defines.NewResponse(200, m_Model.ToString(), "text/html"));
            else
                SendResponse(clientID, Http.Defines.NewResponse(404));
        }

        internal void Update(long deltaTime)
        {
            OnUpdate(deltaTime);
            string? modelUpdate = m_Model.Update(deltaTime);
            if (modelUpdate != null)
                Send(modelUpdate);
        }

        /// <summary>
        /// Function called when updating the controler
        /// </summary>
        /// <param name="deltaTime">Elapsed time in milliseconds since last update</param>
        protected abstract void OnUpdate(long deltaTime);

        private void Generate()
        {
            GenerateModel();
            AfterGenerateModel();
        }

        /// <summary>
        /// Set the favicon of the page to the Quicksand favicon
        /// </summary>
        /// <remarks>
        /// Favicon ICO as been converted to Base64 to be inserted directly into the HTML code instead of being loaded
        /// </remarks>
        protected void SetQuicksandFavicon()
        {
            Head head = m_Model.GetHead();
            Link favicon = new(Link.Type.ICON);
            favicon.SetMediaType("image/x-icon");
            favicon.SetHref(FAVICON);
            head.AddLink(favicon);
        }

        /// <summary>
        /// Function called when generating the HTML model for the first time
        /// </summary>
        protected abstract void GenerateModel();

        /// <summary>
        /// Function called after generating the HTML model for the first time
        /// </summary>
        protected abstract void AfterGenerateModel();
    }
}
