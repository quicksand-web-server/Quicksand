using System.Net.Sockets;
using System.Text;

namespace Quicksand.Web.WebSocket
{
    internal class Protocol : AProtocole
    {
        private readonly StringBuilder m_ReadBuffer = new();

        public Protocol(Socket socket, Client client) : base(socket, client) {}

        internal override void ReadBuffer(byte[] buffer)
        {
            Frame frame = new(buffer);
            m_ReadBuffer.Append(frame.GetContent());
            if (frame.IsFin())
            {
                string message = m_ReadBuffer.ToString();
                if (frame.GetOpCode() == 1) //1 - text message
                    m_Client.OnMessage(message);
                else if (frame.GetOpCode() == 8) //8 - close message
                    m_Client.OnClose(frame.GetStatusCode(), message);
                else if (frame.GetOpCode() == 9) //9 - ping message
                    WriteToWebsocket(new(true, 10, message)); //10 - pong message
                m_ReadBuffer.Clear();
            }
        }

        private void WriteToWebsocket(Frame frame)
        {
            Send(frame.GetBytes());
        }

        internal override void WriteBuffer(string buffer)
        {
            WriteToWebsocket(new(true, 1, buffer));
        }

        public static Http.Response HandleHandshake(Http.Request request)
        {
            Http.Response response = new(request.Version, 101, "Switching Protocols");
            response["Server"] = "Web Overlay HTTP Server";
            response["Content-Type"] = "text/html";
            response["Connection"] = "Upgrade";
            response["Upgrade"] = "websocket";
            response["Sec-WebSocket-Accept"] = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes((string)request["Sec-WebSocket-Key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            return response;
        }
    }
}
