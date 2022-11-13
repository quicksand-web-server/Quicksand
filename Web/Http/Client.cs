namespace Quicksand.Web.Http
{
    /// <summary>
    /// An HTTP Client
    /// </summary>
    public class Client : Web.Client
    {
        /// <summary>
        /// Constructor of a HTTP Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="ip">IP of the server to connect to</param>
        /// <param name="isSecured">Specify if his client is connected to an HTTPS server</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string ip, bool isSecured, int id = 0) :
            base(listener, (isSecured) ? new(string.Format("https://{0}", ip)) : new(string.Format("http://{0}", ip)), id) { }

        /// <summary>
        /// Constructor of a HTTP Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, string url, int id = 0) :
            base(listener, new(url), id) { }

        /// <summary>
        /// Constructor of a HTTP Client
        /// </summary>
        /// <param name="listener">Client listener</param>
        /// <param name="url">URL of the server to connect to</param>
        /// <param name="id">ID of the client (0 by default)</param>
        public Client(AClientListener listener, URL url, int id = 0) : base(listener, url, id) { }
    }
}
