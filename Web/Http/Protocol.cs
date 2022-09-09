using System.Net.Sockets;
using System.Text;

namespace Quicksand.Web.Http
{
    internal class Protocol : AProtocole
    {
        private string m_ReadBuffer = "";
        private Request? m_HoldingRequest = null;
        private byte[] m_RequestBuffer = Array.Empty<byte>();
        private Response? m_HoldingResponse = null;
        private byte[] m_ResponseBuffer = Array.Empty<byte>();

        public Protocol(Stream stream, Client client) : base(stream, client) {}

        private string? GetBody(int bodyLength)
        {
            byte[] readBufferBytes = Encoding.UTF8.GetBytes(m_ReadBuffer);
            if (readBufferBytes.Length >= bodyLength)
            {
                byte[] bodyBytes = readBufferBytes.Take(bodyLength).ToArray();
                m_ReadBuffer = Encoding.UTF8.GetString(readBufferBytes.Skip(bodyLength).ToArray());
                return Encoding.UTF8.GetString(bodyBytes);
            }
            return null;
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        private bool HandleRequestBodyRead(byte[] buffer)
        {
            m_RequestBuffer = m_RequestBuffer.Concat(buffer).ToArray();
            int bodyLength = int.Parse((string)m_HoldingRequest["Content-Length"]);
            if (m_RequestBuffer.Length >= bodyLength)
            {
                byte[] bodyBytes = m_RequestBuffer.Take(bodyLength).ToArray();
                m_ReadBuffer = Encoding.UTF8.GetString(m_RequestBuffer.Skip(bodyLength).ToArray());
                m_HoldingRequest.SetBody(Encoding.UTF8.GetString(bodyBytes));
                m_Client.OnRequest(m_HoldingRequest);
                m_HoldingRequest = null;
                m_RequestBuffer = Array.Empty<byte>();
                return true;
            }
            return false;
        }

        private bool HandleResponseBodyRead(byte[] buffer)
        {
            m_ResponseBuffer = m_ResponseBuffer.Concat(buffer).ToArray();
            int bodyLength = int.Parse((string)m_HoldingResponse["Content-Length"]);
            if (m_ResponseBuffer.Length >= bodyLength)
            {
                byte[] bodyBytes = m_ResponseBuffer.Take(bodyLength).ToArray();
                m_ReadBuffer = Encoding.UTF8.GetString(m_ResponseBuffer.Skip(bodyLength).ToArray());
                m_HoldingResponse.SetBody(Encoding.UTF8.GetString(bodyBytes));
                m_Client.OnResponse(m_HoldingResponse);
                m_HoldingResponse = null;
                m_ResponseBuffer = Array.Empty<byte>();
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
                string? body = GetBody(int.Parse((string)response["Content-Length"]));
                if (body != null)
                {
                    response.SetBody(body);
                    m_Client.OnResponse(response);
                }
                else
                {
                    m_HoldingResponse = response;
                    m_ResponseBuffer = Encoding.UTF8.GetBytes(m_ReadBuffer);
                }
            }
            else
                m_Client.OnResponse(response);
        }

        private void HandleRequest(string data)
        {
            Request request = new(data);
            if (request.HaveHeaderField("Content-Length"))
            {
                string? body = GetBody(Int32.Parse((string)request["Content-Length"]));
                if (body != null)
                {
                    request.SetBody(body);
                    m_Client.OnRequest(request);
                }
                else
                {
                    m_HoldingRequest = request;
                    m_RequestBuffer = Encoding.UTF8.GetBytes(m_ReadBuffer);
                }
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
            catch {}
        }
    }
}
