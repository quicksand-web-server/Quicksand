using System.Text;

namespace Quicksand.Web.Http
{
    internal class HeaderFields
    {
        private readonly Dictionary<string, object> m_Fields = new();
        private readonly Dictionary<string, object> m_LowerFields = new();

        public object this[string key]
        {
            get => GetField(key);
            set => SetField(key, value);
        }

        public HeaderFields() { }
        public HeaderFields(List<string> fields)
        {
            foreach (var field in fields)
            {
                int position = field.IndexOf(": ");
                if (position >= 0)
                {
                    string fieldName = field[..position];
                    string fieldValue = field[(position + 2)..];
                    SetField(fieldName, fieldValue);
                }
            }
        }

        public HeaderFields(HeaderFields other)
        {
            foreach (var field in other.m_Fields)
                SetField(field.Key, field.Value);
        }

        public bool HaveHeaderField(string fieldName) { return m_LowerFields.ContainsKey(fieldName.ToLower()); }

        private void SetField(string key, object value)
        {
            m_Fields[key] = value;
            m_LowerFields[key.ToLower()] = value;
        }

        private object GetField(string key) => m_LowerFields[key.ToLower()];

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
