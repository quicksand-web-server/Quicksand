using System.Net.Sockets;
using System.Text;

namespace Quicksand.Web.Http
{
    internal class Protocol : AProtocole
    {
        private string m_ReadBuffer = "";

        public Protocol(Socket socket, Client client) : base(socket, client) {}

        internal override void ReadBuffer(byte[] buffer)
        {
            m_ReadBuffer += Encoding.Default.GetString(buffer, 0, buffer.Length);
            int position;
            do
            {
                position = m_ReadBuffer.IndexOf(Defines.CRLF + Defines.CRLF);
                if (position >= 0)
                {
                    string data = m_ReadBuffer[..position];
                    m_ReadBuffer = m_ReadBuffer[(position + (Defines.CRLF.Length * 2))..];
                    if (data.StartsWith("HTTP"))
                    {
                        //TODO Handle response
                    }
                    else
                    {
                        Request httpRequest = new(data);
                        //TODO Handle request body
                        m_Client.OnRequest(httpRequest);
                    }
                }
            } while (position >= 0);
        }

        internal override void WriteBuffer(string buffer)
        {
            try
            {
                byte[] data = Encoding.Default.GetBytes(buffer);
                if (m_Socket.Connected)
                    m_Socket.Send(data, data.Length, 0);
            }
            catch {}
        }
    }
}
