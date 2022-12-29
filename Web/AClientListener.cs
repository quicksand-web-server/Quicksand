using Quicksand.Web.WebSocket;

namespace Quicksand.Web
{
    /// <summary>
    /// Class to manage messages received by the client
    /// </summary>
    public abstract class AClientListener
    {
        /// <summary>
        /// Function called when the client disconnect
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        public abstract void OnClientDisconnect(int clientID);

        /// <summary>
        /// Function called when the client receive a websocket message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="message">Websocket message received</param>
        public virtual void OnWebSocketMessage(int clientID, string message) { }

        /// <summary>
        /// Function called when the client connect as a websocket
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="response">HTTP response received</param>
        public virtual void OnWebSocketOpen(int clientID, Http.Response response) { }

        /// <summary>
        /// Function called when the client receive a websocket close message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="code">Close status code received</param>
        /// <param name="closeMessage">Websocket closing message received</param>
        public virtual void OnWebSocketClose(int clientID, short code, string closeMessage) {}

        /// <summary>
        /// Function called when the client receive a websocket error message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="error">Websocket error message received</param>
        public virtual void OnWebSocketError(int clientID, string error) { }

        /// <summary>
        /// Function called when the client receive a websocket frame
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="frame">Websocket frame received</param>
        public virtual void OnWebSocketFrame(int clientID, Frame frame) { }

        /// <summary>
        /// Function called when the client sent a websocket frame
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="frame">Websocket frame sent</param>
        public virtual void OnWebSocketFrameSent(int clientID, Frame frame) { }

        /// <summary>
        /// Function called when the client receive an HTTP request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received</param>
        public virtual void OnHttpRequest(int clientID, Http.Request request) {}

        /// <summary>
        /// Function called when the client receive an HTTP response
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="response">HTTP response received</param>
        public virtual void OnHttpResponse(int clientID, Http.Response response) { }

        /// <summary>
        /// Function called when the client send an HTTP request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received</param>
        public virtual void OnHttpRequestSent(int clientID, Http.Request request) { }

        /// <summary>
        /// Function called when the client send an HTTP response
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="response">HTTP response received</param>
        public virtual void OnHttpResponseSent(int clientID, Http.Response response) { }
    }
}
