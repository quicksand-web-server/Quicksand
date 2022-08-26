namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;script&gt; markup of an HTML document
    /// </summary>
    public class Script : Element
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Script() : base("script")
        {
            AddStrictValidationRule(new()
            {
                {"async", new IsBool()},
                {"crossorigin", new IsInListRule(new(){"anonymous", "use-credentials"})},
                {"defer", new IsBool()},
                {"integrity", new IsNonEmptyStringRule()},
                {"referrerpolicy", new IsInListRule(new(){"no-referrer", "no-referrer-when-downgrade", "origin", "origin-when-cross-origin", "same-origin", "strict-origin", "strict-origin-when-cross-origin", "unsafe-url"})},
                {"src", new IsNonEmptyStringRule()},
                {"type", new IsNonEmptyStringRule()},
            });
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Script(); }

        /// <summary>
        /// Set the content of the script
        /// </summary>
        /// <param name="content">Content of the script</param>
        public void SetScriptContent(string content) { AddContent(content, -1, true); }
        /// <summary>
        /// Specifies that the script is downloaded in parallel to parsing the page, and executed as soon as it is available (before parsing completes) (only for external scripts)
        /// </summary>
        /// <param name="value"></param>
        public void SetAsync(bool value) { SetAttribute("async", value); }
        /// <returns>Return whether that the script is downloaded in parallel to parsing the page, and executed as soon as it is available</returns>
        public bool IsAsync() { return HaveAttribute("async"); }
        /// <summary>
        /// Sets the mode of the request to an HTTP CORS Request
        /// </summary>
        /// <param name="value"></param>
        public void SetCrossOrigin(string value) { SetAttribute("crossorigin", value); }
        /// <returns>Return the mode of the request to an HTTP CORS Request</returns>
        public string GetCrossOrigin() { return GetAttribute("crossorigin"); }
        /// <summary>
        /// Specifies that the script is downloaded in parallel to parsing the page, and executed after the page has finished parsing (only for external scripts)
        /// </summary>
        /// <param name="value"></param>
        public void SetDefer(bool value) { SetAttribute("defer", value); }
        /// <returns>Return whether that the script is downloaded in parallel to parsing the page, and executed after the page has finished parsing</returns>
        public bool GetDefer() { return HaveAttribute("defer"); }
        /// <summary>
        /// Allows a browser to check the fetched script to ensure that the code is never loaded if the source has been manipulated
        /// </summary>
        /// <param name="value"></param>
        public void SetIntegrity(string value) { SetAttribute("integrity", value); }
        /// <returns>Return a filehash to check the fetched script to ensure that the code is never loaded if the source has been manipulated</returns>
        public string GetIntegrity() { return GetAttribute("integrity"); }
        /// <summary>
        /// Specifies which referrer information to send when fetching a script
        /// </summary>
        /// <param name="value"></param>
        public void SetReferrerPolicy(string value) { SetAttribute("referrerpolicy", value); }
        /// <returns>Return which referrer information to send when fetching a script</returns>
        public string GetReferrerPolicy() { return GetAttribute("referrerpolicy"); }
        /// <summary>
        /// Specifies the URL of an external script file
        /// </summary>
        /// <param name="value"></param>
        public void SetSrc(string value) { SetAttribute("src", value); }
        /// <returns>Return the URL of an external script file</returns>
        public string GetSrc() { return GetAttribute("src"); }
        /// <summary>
        /// Specifies the media type of the script
        /// </summary>
        /// <param name="value"></param>
        public void SetMediaType(string value) { SetAttribute("type", value); }
        /// <returns>Return the media type of the script</returns>
        public string GetMediaType() { return GetAttribute("type"); }
    }
}
