namespace Quicksand.Web.Html
{
    public class Base : BaseElement
    {
        public enum Type
        {
            BLANK,
            PARENT,
            SELF,
            TOP,
            NONE
        }

        public Base() : base("base", isEmptyTag: true)
        {
            AddStrictValidationRule(new()
            {
                {"href", new IsNonEmptyStringRule()}, //Specifies the base URL for all relative URLs in the page
                {"target", new IsInListRule<string>(new(){"_blank", "_parent", "_self", "_top"})} //Specifies the default target for all hyperlinks and forms in the page
            });
        }

        public void SetBase(string href, Type target)
        {
            if (string.IsNullOrWhiteSpace(href))
                AddAttribute("href", href);
            else
                RemoveAttribute("href");
            _ = target switch
            {
                Type.BLANK => AddAttribute("target", "_blank"),
                Type.PARENT => AddAttribute("target", "_parent"),
                Type.SELF => AddAttribute("target", "_self"),
                Type.TOP => AddAttribute("target", "_top"),
                _ => RemoveAttribute("target")
            };
        }
    }
}
