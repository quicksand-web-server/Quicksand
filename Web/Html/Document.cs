using System.Text;

namespace Quicksand.Web.Html
{
    public class Document : BaseElement
    {
        private readonly Head m_Head;
        private readonly Body m_Body = new();

        public Document(string title) : base("html")
        {
            SetElementValidationList(new(true) { "head", "body" });
            m_Head = new(title);
            AddChild(m_Head);
            AddChild(m_Body);
        }

        public Head GetHead() { return m_Head; }
        public Body GetBody() { return m_Body; }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine("<!DOCTYPE html>");
            Append(ref builder, 0);
            return builder.ToString();
        }
    }
}
