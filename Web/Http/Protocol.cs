using System.Text;

namespace Quicksand.Web.Http
{
    internal class Protocol : AProtocole
    {
        public Protocol(Stream stream, Client client) : base(stream, client) {}

        internal override void ReadBuffer(byte[] buffer)
        {
            string readBuffer = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            int position = readBuffer.IndexOf(Defines.CRLF + Defines.CRLF);
            if (position >= 0)
            {
                string data = readBuffer[..position];
                readBuffer = readBuffer[(position + (Defines.CRLF.Length * 2))..];
                if (data.StartsWith("HTTP"))
                {
                    Response response = new(data);
                    response.SetBody(readBuffer);
                    m_Client.OnResponse(response);
                }
                else
                {
                    Request request = new(data);
                    request.SetBody(readBuffer);
                    m_Client.OnRequest(request);
                }
            }
        }

        internal override void WriteBuffer(string buffer)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(buffer);
                if (m_Stream.CanWrite)
                    m_Stream.Write(data);
            }
            catch {}
        }
    }
}
