namespace Quicksand.Web
{
    public abstract class AProtocol
    {
        private Stream? m_Stream = null;

        internal void SetStream(Stream stream) => m_Stream = stream;

        protected void Send(byte[] buffer) => m_Stream?.Write(buffer);

        public abstract void ReadBuffer(byte[] buffer);
        public abstract void WriteBuffer(string buffer);
    }
}
