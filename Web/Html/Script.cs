namespace Quicksand.Web.Html
{
    public class Script : BaseElement
    {
        public Script() : base("script")
        {
            AddStrictValidationRule(new()
            {
                {"async", new IsTypeRule<bool>()}, //Specifies that the script is downloaded in parallel to parsing the page, and executed as soon as it is available (before parsing completes) (only for external scripts)
                {"crossorigin", new IsInListRule<string>(new(){"anonymous", "use-credentials"})}, //Sets the mode of the request to an HTTP CORS Request
                {"defer", new IsTypeRule<bool>()}, //Specifies that the script is downloaded in parallel to parsing the page, and executed after the page has finished parsing (only for external scripts)
                {"integrity", new IsNonEmptyStringRule()}, //Allows a browser to check the fetched script to ensure that the code is never loaded if the source has been manipulated
                {"referrerpolicy", new IsInListRule<string>(new(){"no-referrer", "no-referrer-when-downgrade", "origin", "origin-when-cross-origin", "same-origin", "strict-origin", "strict-origin-when-cross-origin", "unsafe-url"})}, //Specifies which referrer information to send when fetching a script
                {"src", new IsNonEmptyStringRule()}, //Specifies the URL of an external script file
                {"type", new IsNonEmptyStringRule()}, //Specifies the media type of the script
            });
        }

        public void SetScriptContent(string content) { AddContent(content); }
        public void SetAsync(bool value) { AddAttribute("async", value); }
        public void SetCrossOrigin(string value) { AddAttribute("crossorigin", value); }
        public void SetDefer(bool value) { AddAttribute("defer", value); }
        public void SetIntegrity(string value) { AddAttribute("integrity", value); }
        public void SetReferrerPolicy(string value) { AddAttribute("referrerpolicy", value); }
        public void SetSrc(string value) { AddAttribute("src", value); }
        public void SetType(string value) { AddAttribute("type", value); }
    }
}
