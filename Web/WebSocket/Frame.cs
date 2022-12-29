using System.Text;

namespace Quicksand.Web.WebSocket
{
    /// <summary>
    /// Class representing a websocket frame
    /// </summary>
    public class Frame
    {
        private readonly bool m_Fin;
        private readonly bool m_Rsv1; //Not used for now
        private readonly bool m_Rsv2; //Not used for now
        private readonly bool m_Rsv3; //Not used for now
        private int m_OpCode;
        private bool m_UseMask;
        private byte[] m_Mask = new byte[4];
        private readonly short m_StatusCode;
        private readonly byte[] m_Content;

        internal Frame(bool fin, int opcode, byte[] content, short statusCode = 0, int? mask = null)
        {
            m_Fin = fin;
            m_Rsv1 = false;
            m_Rsv2 = false;
            m_Rsv3 = false;
            m_OpCode = opcode;
            if (mask != null)
            {
                m_UseMask = true;
                m_Mask = BitConverter.GetBytes(mask.Value);
            }
            else
                m_UseMask = false;
            if (opcode == 8)
                m_StatusCode = statusCode;
            else
                m_StatusCode = 0;
            m_Content = content;
        }

        internal Frame(ref byte[] buffer)
        {
            byte[] frame = buffer;
            m_Fin = (frame[0] & 0b10000000) != 0;
            m_Rsv1 = (frame[0] & 0b01000000) != 0;
            m_Rsv2 = (frame[0] & 0b00100000) != 0;
            m_Rsv3 = (frame[0] & 0b00010000) != 0;
            m_OpCode = frame[0] & 0b00001111;
            m_UseMask = (frame[1] & 0b10000000) != 0;
            ulong payloadLen = (ulong)(buffer[1] & 0b01111111);

            frame = frame[2..];

            if (payloadLen == 126)
            {
                payloadLen = BitConverter.ToUInt16(new byte[] { frame[1], frame[0] }, 0);
                frame = frame[2..];
            }
            else if (payloadLen == 127)
            {
                payloadLen = BitConverter.ToUInt64(new byte[] { frame[7], frame[6], frame[5], frame[4], frame[3], frame[2], frame[1], frame[0] }, 0);
                frame = frame[8..];
            }

            if (m_UseMask)
            {
                m_Mask = new byte[4] { frame[0], frame[1], frame[2], frame[3] };
                frame = frame[4..];
                for (ulong i = 0; i < payloadLen; ++i)
                    frame[i] = (byte)(frame[i] ^ m_Mask[i % 4]);
            }
            else
                m_Mask = Array.Empty<byte>();

            if (m_OpCode == 8)
            {
                m_StatusCode = BitConverter.ToInt16(new byte[2] { frame[1], frame[0] }, 0);
                frame = frame[2..];
                payloadLen -= 2;
            }

            m_Content = frame[..(int)payloadLen];
            buffer = frame[(int)payloadLen..];
        }

        internal bool IsControlFrame() { return m_OpCode >= 8; }
        internal bool IsFin() { return m_Fin; }
        internal bool IsRsv1() { return m_Rsv1; }
        internal bool IsRsv2() { return m_Rsv2; }
        internal bool IsRsv3() { return m_Rsv3; }
        internal void SetOpCode(int opCode) { m_OpCode = opCode; }
        internal int GetOpCode() { return m_OpCode; }
        internal short GetStatusCode() { return m_StatusCode; }
        internal byte[] GetContent() { return m_Content; }
        internal void SetMask(int mask)
        {
            m_UseMask = true;
            m_Mask = BitConverter.GetBytes(mask);
        }

        internal byte[] GetBytes()
        {
            List<byte> bytes = new();
            byte[] bytesRaw = m_Content;

            if (m_OpCode == 8)
            {
                byte[] statusByte = BitConverter.GetBytes(m_StatusCode);
                (statusByte[1], statusByte[0]) = (statusByte[0], statusByte[1]);
                bytesRaw = statusByte.Concat(bytesRaw).ToArray();
            }

            long length = bytesRaw.Length;

            byte firstByte = 0;
            if (m_Fin) firstByte += 128;
            if (m_Rsv1) firstByte += 64;
            if (m_Rsv2) firstByte += 32;
            if (m_Rsv3) firstByte += 16;
            firstByte += (byte)m_OpCode;
            bytes.Add(firstByte);
            if (length <= 125)
                bytes.Add((byte)((m_UseMask) ? length + 128 : length));
            else if (length >= 126 && length <= 65535)
            {
                bytes.Add((byte)((m_UseMask) ? 254 : 126));
                bytes.Add((byte)((length >> 8) & 255));
                bytes.Add((byte)(length & 255));
            }
            else
            {
                bytes.Add((byte)((m_UseMask) ? 255 : 127));
                bytes.Add((byte)((length >> 56) & 255));
                bytes.Add((byte)((length >> 48) & 255));
                bytes.Add((byte)((length >> 40) & 255));
                bytes.Add((byte)((length >> 32) & 255));
                bytes.Add((byte)((length >> 24) & 255));
                bytes.Add((byte)((length >> 16) & 255));
                bytes.Add((byte)((length >> 8) & 255));
                bytes.Add((byte)(length & 255));
            }

            if (m_UseMask)
            {
                for (byte i = 0; i != 4; ++i)
                    bytes.Add(m_Mask[i]);
            }

            for (long i = 0; i < length; i++)
                bytes.Add((m_UseMask) ? (byte)(bytesRaw[i] ^ m_Mask[i % 4]) : bytesRaw[i]);
            return bytes.ToArray();
        }

        /// <returns>String representing the frame</returns>
        public override string ToString()
        {
            return string.Format("Fin: {0}, Op code: {1}, Is mask: {2}, Status code: {3}, Content: {4}",
                (m_Fin) ? "true" : "false", m_OpCode, (m_UseMask) ? "true" : "false", m_StatusCode, Encoding.UTF8.GetString(m_Content)); ;
        }
    }
}
