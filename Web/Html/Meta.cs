namespace Quicksand.Web.Html
{
    public class Meta : BaseElement
    {
        public enum Type
        {
            CHARSET,
            APPLICATION_NAME,
            AUTHOR,
            CONTENT_SECURITY_POLICY,
            CONTENT_TYPE,
            DEFAULT_STYLE,
            DESCRIPTION,
            GENERATOR,
            KEYWORDS,
            REFRESH,
            VIEWPORT
        }

        private readonly Type m_Type;

        public Meta(Type type) : base("meta", isEmptyTag: true)
        {
            m_Type = type;

            AddStrictValidationRule(new()
            {
                {"charset", new IsNonEmptyStringRule()}, //Specifies the character encoding for the HTML document
                {"content", new IsNonEmptyStringRule()}, //Specifies the value associated with the http-equiv or name attribute
                {"http-equiv", new IsInListRule<string>(new(){ "content-security-policy", "content-type", "default-style", "refresh"})}, //Provides an HTTP header for the information/value of the content attribute
                {"name", new IsInListRule<string>(new(){ "application-name", "author", "description", "generator", "keywords", "viewport"})} //Specifies a name for the metadata
            });

            _ = type switch
            {
                Type.APPLICATION_NAME => AddAttribute("name", "application-name"),
                Type.AUTHOR => AddAttribute("name", "author"),
                Type.DESCRIPTION => AddAttribute("name", "description"),
                Type.GENERATOR => AddAttribute("name", "generator"),
                Type.KEYWORDS => AddAttribute("name", "keywords"),
                Type.VIEWPORT => AddAttribute("name", "viewport"),
                Type.CONTENT_SECURITY_POLICY => AddAttribute("http-equiv", "content-security-policy"),
                Type.CONTENT_TYPE => AddAttribute("http-equiv", "content-type"),
                Type.DEFAULT_STYLE => AddAttribute("http-equiv", "default-style"),
                Type.REFRESH => AddAttribute("http-equiv", "refresh"),
                _ => false
            };
        }

        public void SetContent(string content)
        {
            if (m_Type == Type.CHARSET)
                AddAttribute("charset", content);
            else
                AddAttribute("content", content);
        }
    }
}
