using System.Text;

namespace Quicksand.Web.Http
{
    internal class HeaderFields
    {
        private Dictionary<string, object> m_Fields = new();

        public object this[string key]
        {
            get => m_Fields[key.ToLower()];
            set => m_Fields[key.ToLower()] = value;
        }

        public HeaderFields() { }
        public HeaderFields(List<string> fields)
        {
            foreach (var field in fields)
            {
                int position = field.IndexOf(": ");
                if (position >= 0)
                {
                    string fieldName = field[..position].ToLower();
                    string fieldValue = field[(position + 2)..];
                    m_Fields[fieldName] = fieldValue;
                }
            }
        }

        public HeaderFields(HeaderFields other)
        {
            foreach (var field in other.m_Fields)
                m_Fields[field.Key] = field.Value;
        }

        public bool HaveHeaderField(string fieldName) { return m_Fields.ContainsKey(fieldName.ToLower()); }

        public override string ToString()
        {
            StringBuilder builder = new();
            foreach (var field in m_Fields)
            {
                builder.Append(field.Key);
                builder.Append(": ");
                builder.Append(field.Value.ToString());
                builder.Append(Defines.CRLF);
            }
            builder.Append(Defines.CRLF);
            return builder.ToString();
        }
    }
}
