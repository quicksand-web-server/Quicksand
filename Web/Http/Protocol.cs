using System.Text;

namespace Quicksand.Web.Http
{
    internal class Protocol : AProtocol
    {
        private bool m_IsHexa = true;
        private int m_ChunkSize = 0;
        private int m_CurrentChunkSize = 0;
        private byte[] m_ReadBuffer = Array.Empty<byte>();
        private byte[] m_ChunkBuilder = Array.Empty<byte>();
        private Request? m_HoldingRequest = null;
        private Response? m_HoldingResponse = null;
        private readonly Web.Client m_Client;

        public Protocol(Web.Client client) => m_Client = client;

        private string GetBody(int bodyLength)
        {
            byte[] readBufferBytes = m_ReadBuffer;
            byte[] bodyBytes = readBufferBytes[..bodyLength];
            m_ReadBuffer = readBufferBytes[bodyLength..];
            return Encoding.UTF8.GetString(bodyBytes);
        }

        private bool Compare<T>(T[] source, T[] key, int sourceIdx)
        {
            int keyIdx = 0;
            while (keyIdx != key.Length)
            {
                if ((sourceIdx + keyIdx) >= source.Length || !source[sourceIdx + keyIdx]!.Equals(key[keyIdx]))
                    return false;
                ++keyIdx;
            }
            return true;
        }

        private int IndexOf<T>(T[] source, T[] key)
        {
            int sourceIdx = 0;
            while (sourceIdx != source.Length)
            {
                if (Compare<T>(source, key, sourceIdx))
                    return sourceIdx;
                ++sourceIdx;
            }
            return -1;
        }

        private T[][] Split<T>(T[] source, T[] key)
        {
            List<T[]> ret = new();
            int position;
            do
            {
                position = IndexOf(source, key);
                if (position >= 0)
                {
                    ret.Add(source[..position]);
                    source = source[(position + key.Length)..];
                }
            } while (position >= 0);
            ret.Add(source);
            return ret.ToArray();
        }

        private string? HandleBodyRead(byte[] buffer)
        {
            byte[][] chunks = Split(buffer, Defines.BYTES_CRLF);
            foreach (byte[] chunk in chunks)
            {
                if (chunk.Length != 0)
                {
                    if (m_IsHexa)
                    {
                        m_ChunkSize = int.Parse(Encoding.UTF8.GetString(chunk), System.Globalization.NumberStyles.HexNumber);
                        if (m_ChunkSize == 0)
                        {
                            string bodyContent = Encoding.UTF8.GetString(m_ChunkBuilder);
                            m_ChunkBuilder = Array.Empty<byte>();
                            return bodyContent;
                        }
                        m_IsHexa = false;
                    }
                    else
                    {
                        int chunkLength = chunk.Length;
                        m_CurrentChunkSize += chunkLength;
                        int chunkBuilderLength = m_ChunkBuilder.Length;
                        Array.Resize(ref m_ChunkBuilder, chunkBuilderLength + chunkLength);
                        for (int i = 0; i < chunkLength; ++i)
                            m_ChunkBuilder[i + chunkBuilderLength] = chunk[i];
                        if (m_ChunkSize == m_CurrentChunkSize)
                        {
                            m_CurrentChunkSize = 0;
                            m_IsHexa = true;
                        }
                    }
                }
            }
            return null;
        }

        private bool HandleRequestBodyRead(byte[] buffer)
        {
            string? body = HandleBodyRead(buffer);
            if (body != null)
            {
                m_HoldingRequest!.SetBody(body);
                m_Client.OnRequest(m_HoldingRequest);
                m_HoldingRequest = null;
                return true;
            }
            return false;
        }

        private bool HandleResponseBodyRead(byte[] buffer)
        {
            string? body = HandleBodyRead(buffer);
            if (body != null)
            {
                m_HoldingResponse!.SetBody(body);
                m_Client.OnResponse(m_HoldingResponse);
                m_HoldingResponse = null;
                return true;
            }
            return false;
        }

        private void HandleResponse(string data)
        {
            Response response = new(data);
            if (response.HaveHeaderField("Content-Length"))
            {
                response.SetBody(GetBody(int.Parse((string)response["Content-Length"])));
                m_Client.OnResponse(response);
            }
            else if (response.HaveHeaderField("Transfer-Encoding") && ((string)response["Transfer-Encoding"]).ToLower().Contains("chunked"))
            {
                m_HoldingResponse = response;
                HandleBodyRead(m_ReadBuffer);
            }
            else
                m_Client.OnResponse(response);
        }

        private void HandleRequest(string data)
        {
            Request request = new(data);
            if (request.HaveHeaderField("Content-Length"))
            {
                request.SetBody(GetBody(int.Parse((string)request["Content-Length"])));
                m_Client.OnRequest(request);
            }
            else if (request.HaveHeaderField("Transfer-Encoding") && ((string)request["Transfer-Encoding"]).ToLower().Contains("chunked"))
            {
                m_HoldingRequest = request;
                HandleBodyRead(m_ReadBuffer);
            }
            else
                m_Client.OnRequest(request);
        }

        public override void ReadBuffer(byte[] buffer)
        {
            if (m_HoldingRequest != null)
            {
                if (!HandleRequestBodyRead(buffer))
                    return;
            }
            else if (m_HoldingResponse != null)
            {
                if (!HandleResponseBodyRead(buffer))
                    return;
            }
            else
            {
                int bufferLength = buffer.Length;
                int readBufferLength = m_ReadBuffer.Length;
                Array.Resize(ref m_ReadBuffer, readBufferLength + bufferLength);
                for (int i = 0; i < bufferLength; ++i)
                    m_ReadBuffer[i + readBufferLength] = buffer[i];
            }
            int position;
            do
            {
                position = IndexOf(m_ReadBuffer, Defines.DUAL_BYTES_CRLF);
                if (position >= 0)
                {
                    string data = Encoding.UTF8.GetString(m_ReadBuffer[..position]);
                    m_ReadBuffer = m_ReadBuffer[(position + 4)..];
                    if (data.StartsWith("HTTP"))
                        HandleResponse(data);
                    else
                        HandleRequest(data);
                }
            } while (position >= 0);
        }

        public override void WriteBuffer(string buffer)
        {
            try
            {
                Send(Encoding.UTF8.GetBytes(buffer));
            }
            catch { }
        }
    }
}
