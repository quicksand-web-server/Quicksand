namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;meta&gt; markup of an HTML document
    /// </summary>
    public class Meta : Element
    {
        /// <summary>
        /// Type of the metadata
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// charset: Specifies the character encoding for the HTML document
            /// </summary>
            CHARSET,
            /// <summary>
            /// name: Specifies the name of the Web application that the page represents
            /// </summary>
            APPLICATION_NAME,
            /// <summary>
            /// name: Specifies the name of the author of the document
            /// </summary>
            AUTHOR,
            /// <summary>
            /// http-equiv: Specifies a content policy for the document
            /// </summary>
            CONTENT_SECURITY_POLICY,
            /// <summary>
            /// http-equiv: Specifies the character encoding for the document
            /// </summary>
            CONTENT_TYPE,
            /// <summary>
            /// http-equiv: Specified the preferred style sheet to use
            /// </summary>
            DEFAULT_STYLE,
            /// <summary>
            /// name: Specifies a description of the page
            /// </summary>
            DESCRIPTION,
            /// <summary>
            /// name: Specifies one of the software packages used to generate the document
            /// </summary>
            GENERATOR,
            /// <summary>
            /// name: Specifies a comma-separated list of keywords - relevant to the page
            /// </summary>
            KEYWORDS,
            /// <summary>
            /// http-equiv: Defines a time interval for the document to refresh itself
            /// </summary>
            REFRESH,
            /// <summary>
            /// name: Controls the viewport
            /// </summary>
            VIEWPORT
        }

        private readonly Type m_Type;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of the metadata</param>
        public Meta(Type type) : base("meta", isEmptyMarkup: true)
        {
            m_Type = type;
            AddStrictValidationRule(new()
            {
                {"charset", new IsNonEmptyStringRule()},
                {"content", new IsNonEmptyStringRule()},
                {"http-equiv", new IsInListRule(new(){ "content-security-policy", "content-type", "default-style", "refresh"})}, //Provides an HTTP header for the information/value of the content attribute
                {"name", new IsInListRule(new(){ "application-name", "author", "description", "generator", "keywords", "viewport"})} //Specifies a name for the metadata
            });
            _ = type switch
            {
                Type.APPLICATION_NAME => SetAttribute("name", "application-name"),
                Type.AUTHOR => SetAttribute("name", "author"),
                Type.DESCRIPTION => SetAttribute("name", "description"),
                Type.GENERATOR => SetAttribute("name", "generator"),
                Type.KEYWORDS => SetAttribute("name", "keywords"),
                Type.VIEWPORT => SetAttribute("name", "viewport"),
                Type.CONTENT_SECURITY_POLICY => SetAttribute("http-equiv", "content-security-policy"),
                Type.CONTENT_TYPE => SetAttribute("http-equiv", "content-type"),
                Type.DEFAULT_STYLE => SetAttribute("http-equiv", "default-style"),
                Type.REFRESH => SetAttribute("http-equiv", "refresh"),
                _ => false
            };
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Meta(m_Type); }

        /// <summary>
        /// Specifies the value associated with the http-equiv or name attribute or the character encoding for the HTML document
        /// </summary>
        /// <param name="content"></param>
        public void SetContent(string content)
        {
            if (m_Type == Type.CHARSET)
                SetAttribute("charset", content);
            else
                SetAttribute("content", content);
        }

        internal Type GetMetaType() { return m_Type; }
    }
}
