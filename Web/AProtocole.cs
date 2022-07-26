using System.Net.Sockets;

namespace Quicksand.Web
{
    internal abstract class AProtocole
    {
        protected readonly Socket m_Socket;
        protected readonly Client m_Client;

        protected AProtocole(Socket socket, Client client)
        {
            m_Socket = socket;
            m_Client = client;
        }

        protected void Send(byte[] buffer) { m_Socket.Send(buffer); }

        internal abstract void ReadBuffer(byte[] buffer);
        internal abstract void WriteBuffer(string buffer);
    }
}
