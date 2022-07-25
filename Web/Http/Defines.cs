namespace Quicksand.Web.Http
{
    static class Defines
    {
        public static readonly string CRLF = "\r\n";
        public static readonly string VERSION = "HTTP/1.1";

        private static readonly Dictionary<int, string> STATUS = new()
        {
            {200, "Ok"},
            {404, "404 Not Found"},
            {501, "501 Not implemented yet"}
        };

        public static Response? NewResponse(string version, int status, string body = "")
        {
            if (STATUS.TryGetValue(status, out var statusMessage))
                return new(version, status, statusMessage, body);
            return null;
        }
    }
}
