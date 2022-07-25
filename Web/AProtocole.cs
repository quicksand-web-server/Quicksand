using System.Net.Sockets;

namespace Quicksand.Web
{
    internal abstract class AProtocole
    {
        protected readonly Socket m_Socket;

        protected AProtocole(Socket socket)
        {
            m_Socket = socket;
        }

        protected void Send(byte[] buffer) { m_Socket.Send(buffer); }

        internal abstract void ReadBuffer(byte[] buffer);
        internal abstract void WriteBuffer(string buffer);
    }
}
