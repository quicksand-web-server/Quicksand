namespace Quicksand.Web.WebSocket
{
    public interface IListener
    {
        public void OnMessage(string message);
        public void OnClose(short status, string reason);
        public void OnError(string message);
        public void OnPing(string message);
        public void OnPong(string message);
    }
}
