namespace Quicksand.Web.Http
{
    /// <summary>
    /// Contains static method and variables used by HTTP server and client
    /// </summary>
    public static class Defines
    {
        /// <summary>
        /// HTTP CRLF "\r\n"
        /// </summary>
        public static readonly string CRLF = "\r\n";
        /// <summary>
        /// HTTP version of the server "HTTP/1.1"
        /// </summary>
        public static readonly string VERSION = "HTTP/1.1";


        private static readonly Dictionary<int, string> STATUS_BODY = new();
        private static readonly Dictionary<int, string> STATUS = new()
        {
            //1xx Informational response
            {100, "Continue"},
            {101, "Switching Protocols"},
            {102, "Processing"},
            {103, "Early Hints"},
            //2xx Success
            {200, "Ok"},
            {201, "Created"},
            {202, "Accepted"},
            {203, "Non-Authoritative Information"},
            {204, "No Content"},
            {205, "Reset Content"},
            {206, "Partial Content"},
            {207, "Multi-Status"},
            {208, "Already Reported"},
            {226, "IM Used"},
            //3xx Redirection
            {300, "Multiple Choices"},
            {301, "Moved Permanently"},
            {302, "Found"},
            {303, "See Other"},
            {304, "Not Modified"},
            {305, "Use Proxy"},
            {306, "Switch Proxy"},
            {307, "Temporary Redirect "},
            {308, "Permanent Redirect"},
            //4xx client errors
            {400, "Bad Request"},
            {401, "Unauthorized"},
            {402, "Payment Required"},
            {403, "Forbidden"},
            {404, "Not Found"},
            {405, "Method Not Allowed"},
            {406, "Not Acceptable"},
            {407, "Proxy Authentication Required"},
            {408, "Request Timeout"},
            {409, "Conflict"},
            {410, "Gone"},
            {411, "Length Required"},
            {412, "Precondition Failed"},
            {413, "Payload Too Large"},
            {414, "URI Too Long"},
            {415, "Unsupported Media Type"},
            {416, "Range Not Satisfiable"},
            {417, "Expectation Failed"},
            {418, "I'm a teapot"},
            {421, "Misdirected Request"},
            {422, "Unprocessable Entity"},
            {423, "Locked"},
            {424, "Failed Dependency"},
            {425, "Too Early"},
            {426, "Upgrade Required"},
            {428, "Precondition Required"},
            {429, "Too Many Requests"},
            {431, "Request Header Fields Too Large"},
            {451, "Unavailable For Legal Reasons"},
            //5xx Server errors
            {500, "Internal Server Error"},
            {501, "Not implemented"},
            {502, "Bad Gateway"},
            {503, "Service Unavailable"},
            {504, "Gateway Timeout"},
            {505, "HTTP Version Not Supported"},
            {506, "Variant Also Negotiates"},
            {507, "Insufficient Storage"},
            {508, "Loop Detected"},
            {510, "Not Extended"},
            {511, "Network Authentication Required"}
        };

        /// <summary>
        /// Create an HTTP response with the appropriate message depending on the status
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <param name="body">HTML body to display on status</param>
        /// <param name="contentType">MIME type of the content to send (empty by default)</param>
        /// <returns>A new response initialized with appropriate status code and message</returns>
        public static Response? NewResponse(int status, string body, string contentType = "")
        {
            if (STATUS.TryGetValue(status, out var statusMessage))
                return new(status, statusMessage, body, contentType);
            return null;
        }

        /// <summary>
        /// Create an HTTP response with the appropriate message and body depending on the status
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <param name="contentType">MIME type of the content to send (empty by default)</param>
        /// <returns>A new response initialized with appropriate status code, message and body</returns>
        public static Response? NewResponse(int status, string contentType = "")
        {
            if (STATUS.TryGetValue(status, out var statusMessage))
            {
                if (STATUS_BODY.TryGetValue(status, out var body))
                    return new(status, statusMessage, body, contentType);
                else
                    return new(status, statusMessage, "", contentType);
            }
            return null;
        }

        /// <summary>
        /// Set the HTML body for a specific status code
        /// </summary>
        /// <param name="status">Status to set the body for</param>
        /// <param name="body">HTML body to display on status</param>
        public static void SetStatusBody(int status, string body)
        {
            STATUS_BODY[status] = body;
        }

        /// <summary>
        /// Set the HTML body for a specific status code with the content of a file
        /// </summary>
        /// <param name="status">Status to set the body for</param>
        /// <param name="path">Path to the HTML body to display on status</param>
        public static void SetStatusBodyFromFile(int status, string path)
        {
            if (System.IO.File.Exists(path))
                STATUS_BODY[status] = System.IO.File.ReadAllText(path);
        }
    }
}
