namespace Quicksand.Web.Html
{
    public class Head : BaseElement
    {
        private readonly Dictionary<Meta.Type, Meta> m_MetaAttributes = new();
        private Base? m_Base = null;

        public Head(string title) : base("head", allowContent: false)
        {
            AddChild(new Title(title));
            SetElementValidationList(new(true){ "title", "style", "base", "link", "meta", "script", "noscript" });
        }

        public void SetCharset(string charset)
        {
            AddMeta(Meta.Type.CHARSET, charset);
        }

        public void AddMeta(Meta.Type type, string content)
        {
            if (!m_MetaAttributes.ContainsKey(type))
            {
                Meta meta = new(type);
                AddChild(meta);
                m_MetaAttributes[type] = meta;
            }
            m_MetaAttributes[type].SetContent(content);
        }

        public void SetBase(string href = "", Base.Type target = Base.Type.NONE)
        {
            if (m_Base == null)
            {
                m_Base = new();
                AddChild(m_Base);
            }
            m_Base[href] = target;
        }

        public void AddLink(Link link)
        {
            AddChild(link);
        }

        public void AddScript(Script script)
        {
            AddChild(script);
        }
    }
}
