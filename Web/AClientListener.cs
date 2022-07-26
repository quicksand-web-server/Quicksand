namespace Quicksand.Web
{
    /// <summary>
    /// Class to manage messages received by the client
    /// </summary>
    public abstract class AClientListener
    {
        internal void ClientDisconnect(int clientID) { OnClientDisconnect(clientID); }
        /// <summary>
        /// Function called when the client disconnect
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        protected abstract void OnClientDisconnect(int clientID);

        internal void WebSocketMessage(int clientID, string message) { OnWebSocketMessage(clientID, message); }
        /// <summary>
        /// Function called when the client receive a websocket message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="message">Websocket message received</param>
        protected virtual void OnWebSocketMessage(int clientID, string message) {}

        internal void WebSocketClose(int clientID, short code, string closeMessage) { OnWebSocketClose(clientID, code, closeMessage); }
        /// <summary>
        /// Function called when the client receive a websocket close message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="code">Close status code received</param>
        /// <param name="closeMessage">Websocket closing message received</param>
        protected virtual void OnWebSocketClose(int clientID, short code, string closeMessage) {}

        internal void WebSocketError(int clientID, string error) { OnWebSocketError(clientID, error); }
        /// <summary>
        /// Function called when the client receive a websocket error message
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="error">Websocket error message received</param>
        protected virtual void OnWebSocketError(int clientID, string error) {}

        internal void HttpRequest(int clientID, Http.Request request) { OnHttpRequest(clientID, request); }
        /// <summary>
        /// Function called when the client receive an http request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">Http request received</param>
        protected virtual void OnHttpRequest(int clientID, Http.Request request) {}

        internal void HttpResponse(int clientID, Http.Response response) { OnHttpResponse(clientID, response);  }
        /// <summary>
        /// Function called when the client receive an http response
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="response">Http response received</param>
        protected virtual void OnHttpResponse(int clientID, Http.Response response) {}
    }
}
