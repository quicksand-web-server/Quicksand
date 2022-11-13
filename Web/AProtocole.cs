namespace Quicksand.Web
{
    internal abstract class AProtocole
    {
        protected readonly Stream m_Stream;
        protected readonly Client m_Client;

        protected AProtocole(Stream stream, Client client)
        {
            m_Stream = stream;
            m_Client = client;
        }

        protected void Send(byte[] buffer) { m_Stream.Write(buffer); }

        internal abstract void ReadBuffer(byte[] buffer);
        internal abstract void WriteBuffer(string buffer);
    }
}
