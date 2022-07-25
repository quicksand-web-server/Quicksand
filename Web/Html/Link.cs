namespace Quicksand.Web.Html
{
    public class Link : BaseElement
    {
        public enum RelationType
        {
            ALTERNATE,
            AUTHOR,
            DNS_PREFETCH,
            HELP,
            ICON,
            LICENSE,
            NEXT,
            PINGBACK,
            PRECONNECT,
            PREFETCH,
            PRELOAD,
            PRERENDER,
            PREV,
            SEARCH,
            STYLESHEET
        }

        public Link(RelationType relationType) : base("link", isEmptyTag: true)
        {
            AddStrictValidationRule(new()
            {
                {"crossorigin", new IsInListRule<string>(new(){"anonymous", "use-credentials"})}, //Specifies how the element handles cross-origin requests
                {"href", new IsNonEmptyStringRule()}, //Specifies the location of the linked document
                {"hreflang", new IsLangRule()}, //Specifies the language of the text in the linked document
                {"media", new IsNonEmptyStringRule()}, //Specifies on what device the linked document will be displayed
                {"referrerpolicy", new IsInListRule<string>(new(){"no-referrer", "no-referrer-when-downgrade", "origin", "origin-when-cross-origin", "unsafe-url"})}, //Specifies which referrer to use when fetching the resource
                {"rel", new IsInListRule<string>(new(){"alternate", "author", "dns-prefetch", "help", "icon", "license", "next", "pingback", "preconnect", "prefetch", "preload", "prerender", "prev", "search", "stylesheet"})}, //Required. Specifies the relationship between the current document and the linked document
                {"sizes", new IsMatchingRegexRule("^[0-9]+x[0-9]+|any")}, //Specifies the size of the linked resource. Only for rel="icon"
                {"type", new IsNonEmptyStringRule()}, //Specifies the media type of the linked document
            });
            SetRel(relationType);
        }

        public void SetCrossOrigin(string value) { AddAttribute("crossorigin", value); }
        public void SetHref(string value) { AddAttribute("href", value); }
        public void SetHrefLang(string value) { AddAttribute("hreflang", value); }
        public void SetMedia(string value) { AddAttribute("media", value); }
        public void SetReferrerPolicy(string value) { AddAttribute("referrerpolicy", value); }
        public void SetRel(RelationType value)
        {
            AddAttribute("rel", value switch
            {
                RelationType.ALTERNATE => "alternate",
                RelationType.AUTHOR => "author",
                RelationType.DNS_PREFETCH => "dns-prefetch",
                RelationType.HELP => "help",
                RelationType.ICON => "icon",
                RelationType.LICENSE => "license",
                RelationType.NEXT => "next",
                RelationType.PINGBACK => "pingback",
                RelationType.PRECONNECT => "preconnect",
                RelationType.PREFETCH => "prefetch",
                RelationType.PRELOAD => "preload",
                RelationType.PRERENDER => "prerender",
                RelationType.PREV => "prev",
                RelationType.SEARCH => "search",
                RelationType.STYLESHEET => "stylesheet",
                _ => "",
            });
        }
        public void SetSizes(int width, int height) { AddAttribute("sizes", string.Format("{0}x{1}", width, height)); }
        public void SetSizes() { AddAttribute("sizes", "any"); }
        public void SetType(string value) { AddAttribute("type", value); }
    }
}
