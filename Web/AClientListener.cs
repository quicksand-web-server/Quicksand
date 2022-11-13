using Quicksand.Web.WebSocket;

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
        protected virtual void OnWebSocketMessage(int clientID, string message) { }

        internal void WebSocketOpen(int clientID, Http.Response response) { OnWebSocketOpen(clientID, response); }
        /// <summary>
        /// Function called when the client connect as a websocket
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="response">HTTP response received</param>
        protected virtual void OnWebSocketOpen(int clientID, Http.Response response) { }

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
        protected virtual void OnWebSocketError(int clientID, string error) { }

        internal void WebSocketFrame(int clientID, Frame frame) { OnWebSocketFrame(clientID, frame); }
        /// <summary>
        /// Function called when the client receive a websocket frame
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="frame">Websocket frame received</param>
        protected virtual void OnWebSocketFrame(int clientID, Frame frame) { }

        internal void WebSocketFrameSent(int clientID, Frame frame) { OnWebSocketFrameSent(clientID, frame); }
        /// <summary>
        /// Function called when the client sent a websocket frame
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="frame">Websocket frame sent</param>
        protected virtual void OnWebSocketFrameSent(int clientID, Frame frame) { }

        internal void HttpRequest(int clientID, Http.Request request) { OnHttpRequest(clientID, request); }
        /// <summary>
        /// Function called when the client receive an HTTP request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received</param>
        protected virtual void OnHttpRequest(int clientID, Http.Request request) {}

        internal void HttpResponse(int clientID, Http.Response response) { OnHttpResponse(clientID, response);  }
        /// <summary>
        /// Function called when the client receive an HTTP response
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="response">HTTP response received</param>
        protected virtual void OnHttpResponse(int clientID, Http.Response response) { }

        internal void HttpRequestSent(int clientID, Http.Request request) { OnHttpRequestSent(clientID, request); }
        /// <summary>
        /// Function called when the client send an HTTP request
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received</param>
        protected virtual void OnHttpRequestSent(int clientID, Http.Request request) { }

        internal void HttpResponseSent(int clientID, Http.Response response) { OnHttpResponseSent(clientID, response); }
        /// <summary>
        /// Function called when the client send an HTTP response
        /// </summary>
        /// <param name="clientID">ID of the client</param>
        /// <param name="response">HTTP response received</param>
        protected virtual void OnHttpResponseSent(int clientID, Http.Response response) { }
    }
}
