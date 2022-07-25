using System.Text;

namespace Quicksand.Web.Http
{
    public class Request
    {
        private readonly string m_Method;
        private readonly string m_Path;
        private readonly string m_Version;
        private readonly HeaderFields m_HeaderFields; //All header fields value will be stored as string in request
        private readonly string m_Body = "";

        public Request(string request)
        {
            List<string> attributes = request.Split(new string[] { Defines.CRLF }, StringSplitOptions.None).ToList();
            string[] requestLine = attributes[0].Trim().Split(' ');
            m_Method = requestLine[0];
            m_Path = requestLine[1];
            m_Version = requestLine[2];
            attributes.RemoveAt(0);
            m_HeaderFields = new(attributes);
        }

        public Request(Request request, string body)
        {
            m_Method = request.Method;
            m_Path = request.Path;
            m_Version = request.Version;
            m_HeaderFields = new(request.m_HeaderFields);
            m_Body = body;
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append(m_Method);
            builder.Append(' ');
            builder.Append(m_Path);
            builder.Append(' ');
            builder.Append(m_Version);
            builder.Append(Defines.CRLF);
            builder.Append(m_HeaderFields);
            builder.Append(m_Body);
            return builder.ToString();
        }

        public bool HaveHeaderField(string headerFieldName) { return m_HeaderFields.HaveHeaderField(headerFieldName); }

        public string Method { get => m_Method; }
        public string Path { get => m_Path; }
        public string Version { get => m_Version; }
        public string Body { get => m_Body; }
        public object this[string key]
        {
            get => m_HeaderFields[key];
            set => m_HeaderFields[key] = value;
        }
    }
}
