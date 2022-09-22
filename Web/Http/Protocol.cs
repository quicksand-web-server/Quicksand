using System;
using System.Text;

namespace Quicksand.Web.Http
{
    internal class Protocol : AProtocole
    {
        private bool m_IsHexa = true;
        private int m_ChunkSize = 0;
        private int m_CurrentChunkSize = 0;
        private string m_ReadBuffer = "";
        private Request? m_HoldingRequest = null;
        private Response? m_HoldingResponse = null;
        private readonly StringBuilder m_ChunkBuilder = new();

        public Protocol(Stream stream, Client client) : base(stream, client) { }

        private string GetBody(int bodyLength)
        {
            byte[] readBufferBytes = Encoding.UTF8.GetBytes(m_ReadBuffer);
            byte[] bodyBytes = readBufferBytes.Take(bodyLength).ToArray();
            m_ReadBuffer = Encoding.UTF8.GetString(readBufferBytes.Skip(bodyLength).ToArray());
            return Encoding.UTF8.GetString(bodyBytes);
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        private string? HandleBodyRead(byte[] buffer)
        {
            string ret = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            string[] chunks = ret.Split(Defines.CRLF);
            foreach (string chunk in chunks)
            {
                if (!string.IsNullOrEmpty(chunk))
                {
                    if (m_IsHexa)
                    {
                        m_ChunkSize = int.Parse(chunk, System.Globalization.NumberStyles.HexNumber);
                        if (m_ChunkSize == 0)
                        {
                            string bodyContent = m_ChunkBuilder.ToString();
                            m_ChunkBuilder.Clear();
                            return bodyContent;
                        }
                        m_IsHexa = false;
                    }
                    else
                    {
                        m_CurrentChunkSize += Encoding.UTF8.GetBytes(chunk).Length;
                        m_ChunkBuilder.Append(chunk);
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
                m_HoldingRequest.SetBody(body);
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
                m_HoldingResponse.SetBody(body);
                m_Client.OnResponse(m_HoldingResponse);
                m_HoldingResponse = null;
                return true;
            }
            return false;
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
                HandleBodyRead(Encoding.UTF8.GetBytes(m_ReadBuffer));
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
                HandleBodyRead(Encoding.UTF8.GetBytes(m_ReadBuffer));
            }
            else
                m_Client.OnRequest(request);
        }

        internal override void ReadBuffer(byte[] buffer)
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
                m_ReadBuffer += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            int position;
            do
            {
                position = m_ReadBuffer.IndexOf(Defines.CRLF + Defines.CRLF);
                if (position >= 0)
                {
                    string data = m_ReadBuffer[..position];
                    m_ReadBuffer = m_ReadBuffer[(position + (Defines.CRLF.Length * 2))..];
                    if (data.StartsWith("HTTP"))
                        HandleResponse(data);
                    else
                        HandleRequest(data);
                }
            } while (position >= 0);
        }

        internal override void WriteBuffer(string buffer)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(buffer);
                if (m_Stream.CanWrite)
                    m_Stream.Write(data);
            }
            catch { }
        }
    }
}
