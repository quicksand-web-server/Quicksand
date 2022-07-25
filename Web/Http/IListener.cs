namespace Quicksand.Web.Http
{
    public interface IListener
    {
        public void OnRequest(Request request);
        public void OnResponse(Response response);
    }
}
