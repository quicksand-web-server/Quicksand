using System.Text;

namespace Quicksand.Web.WebSocket
{
    internal class Protocol : AProtocol
    {
        private byte[] m_FragmentBuffer = Array.Empty<byte>();
        private int m_LastOpCode = 0;
        private readonly int m_FragmentSize;
        private readonly Web.Client m_Client;

        public Protocol(Web.Client client, int fragmentSize)
        {
            m_Client = client;
            m_FragmentSize = fragmentSize;
        }

        private List<Frame> FragmentFrame(bool fin, int opCode, byte[] message)
        {
            List<Frame> frames = new();
            int nbFragment = (m_FragmentSize > 0 && opCode < 8) ? message.Length / m_FragmentSize : 0;
            if (nbFragment == 0)
            {
                frames.Add(new(fin, opCode, message));
                return frames;
            }
            for (int i = 0; i <= nbFragment; i++)
            {
                byte[] frameBytes = message[(i * m_FragmentSize)..((i + 1) * m_FragmentSize)];
                if (i == 0)
                    frames.Add(new(false, opCode, frameBytes)); //First frame
                else if (i == nbFragment)
                    frames.Add(new(true, 0, frameBytes)); //Last frame
                else
                    frames.Add(new(false, 0, frameBytes));
            }
            return frames;
        }

        public override void ReadBuffer(byte[] buffer)
        {
            do
            {
                Frame frame = new(ref buffer);
                m_Client.OnWebsocketFrameReceived(frame);
                bool isControlFrame = frame.IsControlFrame();
                if (!isControlFrame) //Control frame cannot be fragmented
                {
                    byte[] frameContent = frame.GetContent();
                    int frameContentLegth = frameContent.Length;
                    int bufferLength = m_FragmentBuffer.Length;
                    Array.Resize(ref m_FragmentBuffer, bufferLength + frameContentLegth);
                    for (int i = 0; i < frameContentLegth; ++i)
                        m_FragmentBuffer[i + bufferLength] = frameContent[i];
                }
                if (frame.IsFin() || isControlFrame)
                {
                    int opCode = (isControlFrame || m_LastOpCode == 0) ? frame.GetOpCode() : m_LastOpCode;
                    string message = Encoding.UTF8.GetString((isControlFrame) ? frame.GetContent() : m_FragmentBuffer);
                    switch (opCode)
                    {
                        case 1: m_Client.OnMessage(message); break; //1 - text message
                        case 8: m_Client.OnClose(frame.GetStatusCode(), message); break; //8 - close message
                        case 9: //9 - ping message
                            frame.SetOpCode(10); //10 - pong message
                            SendFrame(frame);
                            break;
                    }
                    if (!isControlFrame)
                    {
                        m_FragmentBuffer = Array.Empty<byte>();
                        m_LastOpCode = 0;
                    }
                }
                else if (frame.GetOpCode() != 0)
                    m_LastOpCode = frame.GetOpCode();
            } while (buffer.Length > 0);
        }

        private void SendFrames(List<Frame> frames)
        {
            foreach (Frame frame in frames)
                SendFrame(frame);
        }

        private void SendFrame(Frame frame)
        {
            if (!m_Client.IsServer()) //We mask the frame if it's send by a client
                frame.SetMask(new Random().Next());
            Send(frame.GetBytes());
            m_Client.OnWebsocketFrameSent(frame);
        }

        public override void WriteBuffer(string buffer) => SendFrames(FragmentFrame(true, 1, Encoding.UTF8.GetBytes(buffer)));

        public static Http.Response HandleHandshake(Http.Request request)
        {
            Http.Response response = new(101, "Switching Protocols");
            response["Server"] = "Web Overlay HTTP Server";
            response["Content-Type"] = "text/html";
            response["Connection"] = "Upgrade";
            response["Upgrade"] = "websocket";
            response["Sec-WebSocket-Accept"] = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes((string)request["Sec-WebSocket-Key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            return response;
        }
    }
}
