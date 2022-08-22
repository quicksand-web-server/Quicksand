namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;base&gt; markup of an HTML document
    /// </summary>
    public class Base : Element
    {
        /// <summary>
        /// The default target for all hyperlinks and forms in the page
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Opens the link in a new window or tab
            /// </summary>
            BLANK,
            /// <summary>
            /// Default. Opens the link in the same frame as it was clicked
            /// </summary>
            PARENT,
            /// <summary>
            /// Opens the link in the parent frame
            /// </summary>
            SELF,
            /// <summary>
            /// Opens the link in the full body of the window
            /// </summary>
            TOP
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Base() : base("base", isEmptyMarkup: true)
        {
            AddStrictValidationRule(new()
            {
                {"href", new IsNonEmptyStringRule()},
                {"target", new IsInListRule(new(){"_blank", "_parent", "_self", "_top"})}
            });
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Base(); }

        /// <summary>
        /// Set the &lt;base&gt; information
        /// </summary>
        /// <param name="href">Specifies the base URL for all relative URLs in the page</param>
        /// <param name="target">Specifies the default target for all hyperlinks and forms in the page</param>
        public void SetBase(string href, Type target)
        {
            if (string.IsNullOrWhiteSpace(href))
                SetAttribute("href", href);
            else
                RemoveAttribute("href");
            _ = target switch
            {
                Type.BLANK => SetAttribute("target", "_blank"),
                Type.PARENT => SetAttribute("target", "_parent"),
                Type.SELF => SetAttribute("target", "_self"),
                Type.TOP => SetAttribute("target", "_top"),
                _ => RemoveAttribute("target")
            };
        }
    }
}
