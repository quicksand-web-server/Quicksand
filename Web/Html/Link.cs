namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;link&gt; markup of an HTML document
    /// </summary>
    public class Link : Element
    {
        /// <summary>
        /// Relationship between the current document and the linked document/resource
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Provides a link to an alternate version of the document (i.e. print page, translated or mirror)
            /// </summary>
            ALTERNATE,
            /// <summary>
            /// Provides a link to the author of the document
            /// </summary>
            AUTHOR,
            /// <summary>
            /// Specifies that the browser should preemptively perform DNS resolution for the target resource's origin
            /// </summary>
            DNS_PREFETCH,
            /// <summary>
            /// Provides a link to a help document
            /// </summary>
            HELP,
            /// <summary>
            /// Imports an icon to represent the document
            /// </summary>
            ICON,
            /// <summary>
            /// Provides a link to copyright information for the document
            /// </summary>
            LICENSE,
            /// <summary>
            /// Provides a link to the next document in the series
            /// </summary>
            NEXT,
            /// <summary>
            /// Provides the address of the pingback server that handles pingbacks to the current document
            /// </summary>
            PINGBACK,
            /// <summary>
            /// Specifies that the browser should preemptively connect to the target resource's origin
            /// </summary>
            PRECONNECT,
            /// <summary>
            /// Specifies that the browser should preemptively fetch and cache the target resource as it is likely to be required for a follow-up navigation
            /// </summary>
            PREFETCH,
            /// <summary>
            /// Specifies that the browser agent must preemptively fetch and cache the target resource for current navigation according to the destination given by the "as" attribute (and the priority associated with that destination)
            /// </summary>
            PRELOAD,
            /// <summary>
            /// Specifies that the browser should pre-render (load) the specified webpage in the background. So, if the user navigates to this page, it speeds up the page load (because the page is already loaded). Warning! This wastes the user's bandwidth! Only use prerender if you are absolutely sure that the webpage is required at some point in the user's journey
            /// </summary>
            PRERENDER,
            /// <summary>
            /// Indicates that the document is a part of a series, and that the previous document in the series is the referenced document
            /// </summary>
            PREV,
            /// <summary>
            /// Provides a link to a resource that can be used to search through the current document and its related pages
            /// </summary>
            SEARCH,
            /// <summary>
            /// Imports a style sheet
            /// </summary>
            STYLESHEET
        }

        private readonly Type m_RelationType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="relationType">Specifies the relationship between the current document and the linked document</param>
        public Link(Type relationType) : base("link", isEmptyMarkup: true)
        {
            m_RelationType = relationType;
            AddStrictValidationRule(new()
            {
                {"crossorigin", new IsInListRule(new(){"anonymous", "use-credentials"})},
                {"href", new IsNonEmptyStringRule()},
                {"hreflang", new IsLangRule()},
                {"media", new IsNonEmptyStringRule()},
                {"referrerpolicy", new IsInListRule(new(){"no-referrer", "no-referrer-when-downgrade", "origin", "origin-when-cross-origin", "unsafe-url"})},
                {"rel", new IsInListRule(new(){"alternate", "author", "dns-prefetch", "help", "icon", "license", "next", "pingback", "preconnect", "prefetch", "preload", "prerender", "prev", "search", "stylesheet"})},
                {"sizes", new IsMatchingRegexRule("^[0-9]+x[0-9]+|any")},
                {"type", new IsNonEmptyStringRule()},
            });
            SetRel(relationType);
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Link(m_RelationType); }

        /// <summary>
        /// Specifies how the element handles cross-origin requests
        /// </summary>
        /// <param name="value"></param>
        public void SetCrossOrigin(string value) { SetAttribute("crossorigin", value); }
        /// <returns>Return the mode of the request to an HTTP CORS Request</returns>
        public string GetCrossOrigin() { return GetAttribute("crossorigin"); }
        /// <summary>
        /// Specifies the location of the linked document
        /// </summary>
        /// <param name="value"></param>
        public void SetHref(string value) { SetAttribute("href", value); }
        /// <returns>Return the location of the linked document</returns>
        public string GetHref() { return GetAttribute("href"); }
        /// <summary>
        /// Specifies the language of the text in the linked document
        /// </summary>
        /// <param name="value"></param>
        public void SetHrefLang(string value) { SetAttribute("hreflang", value); }
        /// <returns>Return the language of the text in the linked document</returns>
        public string GetHrefLang() { return GetAttribute("href"); }
        /// <summary>
        /// Specifies on what device the linked document will be displayed
        /// </summary>
        /// <param name="value"></param>
        public void SetMedia(string value) { SetAttribute("media", value); }
        /// <returns>Return on what device the linked document will be displayed</returns>
        public string GetMedia() { return GetAttribute("media"); }
        /// <summary>
        /// Specifies which referrer to use when fetching the resource
        /// </summary>
        /// <param name="value"></param>
        public void SetReferrerPolicy(string value) { SetAttribute("referrerpolicy", value); }
        /// <returns>Return which referrer information to send when fetching a script</returns>
        public string GetReferrerPolicy() { return GetAttribute("referrerpolicy"); }
        /// <summary>
        /// Specifies the relationship between the current document and the linked document
        /// </summary>
        /// <param name="value"></param>
        public void SetRel(Type value)
        {
            SetAttribute("rel", value switch
            {
                Type.ALTERNATE => "alternate",
                Type.AUTHOR => "author",
                Type.DNS_PREFETCH => "dns-prefetch",
                Type.HELP => "help",
                Type.ICON => "icon",
                Type.LICENSE => "license",
                Type.NEXT => "next",
                Type.PINGBACK => "pingback",
                Type.PRECONNECT => "preconnect",
                Type.PREFETCH => "prefetch",
                Type.PRELOAD => "preload",
                Type.PRERENDER => "prerender",
                Type.PREV => "prev",
                Type.SEARCH => "search",
                Type.STYLESHEET => "stylesheet",
                _ => throw new NotImplementedException(),
            });
        }
        /// <returns>Return the relationship between the current document and the linked document</returns>
        public Type GetRel()
        {
            return GetAttribute("rel") switch
            {
                "alternate" => Type.ALTERNATE,
                "author" => Type.AUTHOR,
                "dns-prefetch" => Type.DNS_PREFETCH,
                "help" => Type.HELP,
                "icon" => Type.ICON,
                "license" => Type.LICENSE,
                "next" => Type.NEXT,
                "pingback" => Type.PINGBACK,
                "preconnect" => Type.PRECONNECT,
                "prefetch" => Type.PREFETCH,
                "preload" => Type.PRELOAD,
                "prerender" => Type.PRERENDER,
                "prev" => Type.PREV,
                "search" => Type.SEARCH,
                "stylesheet" => Type.STYLESHEET,
                _ => throw new NotImplementedException()
            };
        }
        /// <summary>
        /// Specifies the size of the linked resource. Only for rel="icon"
        /// </summary>
        /// <param name="width">Width of the resource</param>
        /// <param name="height">Height of the resource</param>
        public void SetSizes(int width, int height) { SetAttribute("sizes", string.Format("{0}x{1}", width, height)); }
        /// <summary>
        /// Set the size to any of the linked resource. Only for rel="icon"
        /// </summary>
        public void SetSizes() { SetAttribute("sizes", "any"); }
        /// <returns>Return the sizes of icons for visual media</returns>
        public string GetSizes() { return GetAttribute("sizes"); }
        /// <summary>
        /// Specifies the media type of the script
        /// </summary>
        /// <param name="value"></param>
        public void SetMediaType(string value) { SetAttribute("type", value); }
        /// <returns>Return the media type of the script</returns>
        public string GetMediaType() { return GetAttribute("type"); }
    }
}
