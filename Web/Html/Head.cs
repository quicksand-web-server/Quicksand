namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;head&gt; markup of an HTML document
    /// </summary>
    public class Head : Element
    {
        private readonly Dictionary<Meta.Type, Meta> m_MetaAttributes = new();
        private Base? m_Base = null;

        internal Head() : base("head", allowContent: false)
        {
            AddElementToValidationList(new() { "title", "style", "base", "link", "meta", "script", "noscript" });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Head(string title) : this()
        {
            AddChild(new Title(title));
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Head(); }

        /// <summary>
        /// Function called when a child is added to a duplicate
        /// </summary>
        /// <param name="original">Duplicated element</param>
        /// <param name="child">Child added</param>
        protected override void OnDuplicateElementAdded(ElementBase original, ElementBase child)
        {
            if (child is Base newBase)
                m_Base = newBase;
            else if (child is Meta meta)
                m_MetaAttributes[meta.GetMetaType()] = meta;
        }

        /// <summary>
        /// Set the charset of the HTML document
        /// </summary>
        /// <param name="charset">Charset of the HTML document</param>
        public void SetCharset(string charset)
        {
            AddMeta(Meta.Type.CHARSET, charset);
        }

        /// <summary>
        /// Add of a &lt;meta&gt; markup to the &lt;head&gt; markup
        /// </summary>
        /// <param name="type">Type of the &lt;meta&gt; markup</param>
        /// <param name="content">Content of the &lt;meta&gt; markup</param>
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

        /// <summary>
        /// Add of a &lt;base&gt; markup to the &lt;head&gt; markup
        /// </summary>
        /// <param name="target">Type of the &lt;base&gt; markup</param>
        /// <param name="href">Href of the &lt;base&gt; markup</param>
        public void SetBase(Base.Type target, string href = "")
        {
            if (m_Base == null)
            {
                m_Base = new();
                AddChild(m_Base);
            }
            m_Base.SetBase(href, target);
        }

        /// <summary>
        /// Add of a &lt;link&gt; markup to the &lt;head&gt; markup
        /// </summary>
        /// <param name="link">Link to add to the &lt;head&gt; markup</param>
        public void AddLink(Link link)
        {
            AddChild(link);
        }

        /// <summary>
        /// Add of a &lt;script&gt; markup to the &lt;head&gt; markup
        /// </summary>
        /// <param name="script">Script to add to the &lt;head&gt; markup</param>
        public void AddScript(Script script)
        {
            AddChild(script);
        }
    }
}
