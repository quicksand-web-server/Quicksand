using System.Xml.Linq;

namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing an element of an HTML document with accessor to general HTML attributes
    /// </summary>
    public abstract class Element : ElementBase
    {
        private static readonly List<Tuple<string, ValidationList<string>>> ms_EventsRules = new()
        {
            //Window events
            {new("onafterprint", new(true){"body"})},
            {new("onbeforeprint", new(true){"body"})},
            {new("onbeforeunload", new(true){"body"})},
            {new("onhashchange", new(true){"body"})},
            {new("onload", new(true){"body", "frame", "frameset", "iframe", "img", "input", "link", "script", "style"})},
            {new("onmessage", new(true){"body"})},
            {new("onoffline", new(true){"body"})},
            {new("ononline", new(true){"body"})},
            {new("onpagehide", new(true){"body"})},
            {new("onpageshow", new(true){"body"})},
            {new("onpopstate", new(true){"body"})},
            {new("onresize", new(true){"body"})},
            {new("onstorage", new(true){"body"})},
            {new("onunload", new(true){"body"})},

            //Form events
            {new("onblur", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onchange", new(true){"input", "select", "textarea"})},
            {new("oncontextmenu", new(false))},
            {new("onfocus", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onimput", new(true){"input", "textarea"})},
            {new("oninvalid", new(true){"input"})},
            {new("onreset", new(true){"form"})},
            {new("onsearch", new(true){"input"})},
            {new("onselect", new(true){"input", "textarea"})},
            {new("onsubmit", new(true){"form"})},

            //Keyboard events
            {new("onkeydown", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onkeypress", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onkeyup", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},

            //Mouse events
            {new("onclick", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("ondblclick", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onmousedown", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onmousemove", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onmouseout", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onmouseover", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onmouseup", new(false){"base", "bdo", "br", "head", "html", "iframe", "meta", "param", "script", "style", "title"})},
            {new("onwheel", new(false))},

            //Drag events
            {new("ondrag", new(false))},
            {new("ondragend", new(false))},
            {new("ondragenter", new(false))},
            {new("ondragleave", new(false))},
            {new("ondragover", new(false))},
            {new("ondragstart", new(false))},
            {new("ondrop", new(false))},
            {new("onscroll", new(true){"address", "blockquote", "body", "caption", "center", "dd", "dir", "div", "dl", "dt", "fieldset", "form", "h1> to <h6", "html", "li", "menu", "object", "ol", "p", "pre", "select", "tbody", "textarea", "tfoot", "thead", "ul"})},

            //Clipboard events
            {new("oncopy", new(false))},
            {new("oncut", new(false))},
            {new("onpaste", new(false))},

            //Media events
            {new("onabort", new(false))},
            {new("oncanplay", new(false))},
            {new("oncanplaythrough", new(false))},
            {new("oncuechange", new(false))},
            {new("ondurationchange", new(false))},
            {new("onemptied", new(false))},
            {new("onended", new(false))},
            {new("onerror", new(false))},
            {new("onloadeddata", new(false))},
            {new("onloadedmetadata", new(false))},
            {new("onloadstart", new(false))},
            {new("onpause", new(false))},
            {new("onplay", new(false))},
            {new("onplaying", new(false))},
            {new("onprogress", new(false))},
            {new("onratechange", new(false))},
            {new("onseeked", new(false))},
            {new("onseeking", new(false))},
            {new("onstalled", new(false))},
            {new("onsuspend", new(false))},
            {new("ontimeupdate", new(false))},
            {new("onvolumechange", new(false))},
            {new("onwaiting", new(false))},

            //Misc events
            {new("ontoggle", new(true){"details"})}
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Type of the element</param>
        /// <param name="isEmptyMarkup">Specify if the element cannot have child/content (false by default)</param>
        /// <param name="allowContent">Specify if the element can have content (true by default)</param>
        /// <param name="allowUnknownAttributes">Specify if the element allow attributes it doesn't know (false by default)</param>
        protected Element(string name, bool isEmptyMarkup = false, bool allowContent = true, bool allowUnknownAttributes = false) : base(name, isEmptyMarkup, allowContent, allowUnknownAttributes)
        {
            AddStrictValidationRule(new()
            {
                {"accesskey", new IsChar()},
                {"class", new IsMatchingRegexRule("^[a-zA-Z]([a-zA-Z-_])*")},
                {"contenteditable", new IsBool()},
                {"dir", new IsInListRule(new(){"ltr", "rtl", "auto"})},
                {"draggable", new IsBool()},
                {"hidden", new IsBool()},
                {"id", new IsNonEmptyWordRule()},
                {"lang", new IsLangRule()},
                {"spellcheck", new IsBool()},
                {"style", new IsNonEmptyStringRule()},
                {"tabindex", new IsInt()},
                {"title", new IsNonEmptyStringRule()},
                {"translate", new IsBool()}
            });

            AddRegexValidationRule("data-*", new DataAttributeRule()); //Used to store custom data private to the page or application

            SetID(Guid.NewGuid().ToString());

            foreach (var eventRule in ms_EventsRules)
            {
                bool isAllowed = eventRule.Item2.Validate(GetName());
                AddStrictValidationRule(eventRule.Item1, new EventAttributeRule(isAllowed));
                if (isAllowed && eventRule.Item1 == "oncontextmenu")
                    AddStrictValidationRule("contextmenu", new IsNonEmptyStringRule());
            }
        }

        /// <summary>
        /// Specifies a shortcut key to activate/focus an element
        /// </summary>
        /// <param name="value"></param>
        public void SetAccessKey(char value) { _ = SetAttribute("accesskey", new string(value, 1)); }
        /// <returns>Return a shortcut key to activate/focus an element</returns>
        public char GetAccessKey() { return GetAttribute("accesskey")[0]; }
        /// <summary>
        /// Specifies one or more classnames for an element
        /// </summary>
        /// <param name="value"></param>
        public void SetClass(string value) { _ = SetAttribute("class", value); }
        /// <returns>Return one or more classnames for an element</returns>
        public string GetClass() { return GetAttribute("class"); }
        /// <summary>
        /// Specifies whether the content of an ElementBase is editable or not
        /// </summary>
        /// <param name="value"></param>
        public void SetContentEditable(bool value) { _ = SetAttribute("contenteditable", value); }
        /// <returns>Return whether the content of an ElementBase is editable or not</returns>
        public bool IsContentEditable() { return HaveAttribute("contenteditable"); }
        /// <summary>
        /// Used to store custom data private to the page or application
        /// </summary>
        /// <param name="name">Name of the data attribute Note: Attribute will be named data-{name}</param>
        /// <param name="value">Value of the data attribute</param>
        public void SetData(string name, string value) { _ = SetAttribute(string.Format("data-{0}", name), value); }
        /// <param name="name">Name of the data attribute Note: Attribute will be searched as data-{name}</param>
        /// <returns>Return the stored custom data private to the page or application</returns>
        public string GetData(string name) { return GetAttribute(string.Format("data-{0}", name)); }
        /// <summary>
        /// Specifies the text direction for the content in an element
        /// </summary>
        /// <param name="value"></param>
        public void SetDir(string value) { _ = SetAttribute("dir", value); }
        /// <returns>Return the text direction for the content in an element</returns>
        public string GetDir() { return GetAttribute("dir"); }
        /// <summary>
        /// Specifies whether an element is draggable or not
        /// </summary>
        /// <param name="value"></param>
        public void SetDraggable(bool value) { _ = SetAttribute("draggable", value); }
        /// <returns>Return whether an element is draggable or not</returns>
        public bool IsDraggable() { return HaveAttribute("draggable"); }
        /// <summary>
        /// Specifies that an element is not yet, or is no longer, relevant
        /// </summary>
        /// <param name="value"></param>
        public void SetHidden(bool value) { _ = SetAttribute("hidden", value); }
        /// <returns>Return that an element is not yet, or is no longer, relevant</returns>
        public bool IsHidden() { return HaveAttribute("hidden"); }
        /// <summary>
        /// Specifies a unique id for an element
        /// </summary>
        /// <param name="value"></param>
        public void SetID(string value) { _ = SetAttribute("id", value); }
        /// <returns>Return a unique id for an element</returns>
        public string GetID() { return GetAttribute("id"); }
        /// <summary>
        /// Specifies the language of the element's content
        /// </summary>
        /// <param name="value"></param>
        public void SetLang(string value) { _ = SetAttribute("lang", value); }
        /// <returns>Return the language of the element's content</returns>
        public string GetLang() { return GetAttribute("lang"); }
        /// <summary>
        /// Specifies whether the element is to have its spelling and grammar checked or not
        /// </summary>
        /// <param name="value"></param>
        public void SetSpellcheck(bool value) { _ = SetAttribute("spellcheck", value); }
        /// <returns>Return whether the element is to have its spelling and grammar checked or not</returns>
        public bool IsSpellcheck() { return HaveAttribute("spellcheck"); }
        /// <summary>
        /// Specifies an inline CSS style for an element
        /// </summary>
        /// <param name="value"></param>
        public void SetStyle(string value) { _ = SetAttribute("style", value); }
        /// <returns>Return an inline CSS style for an element</returns>
        public string GetStyle() { return GetAttribute("style"); }
        /// <summary>
        /// Specifies the tabbing order of an element
        /// </summary>
        /// <param name="value"></param>
        public void SetTabIndex(int value) { _ = SetAttribute("tabindex", value); }
        /// <returns>Return the tabbing order of an element</returns>
        public int GetTabIndex() { return int.Parse(GetAttribute("tabindex")); }
        /// <summary>
        /// Specifies extra information about an element
        /// </summary>
        /// <param name="value"></param>
        public void SetTitle(string value) { _ = SetAttribute("title", value); }
        /// <returns>Return extra information about an element</returns>
        public string GetTitle() { return GetAttribute("title"); }
        /// <summary>
        /// Specifies whether the content of an element should be translated or not
        /// </summary>
        /// <param name="value"></param>
        public void SetTranslate(bool value) { _ = SetAttribute("translate", value); }
        /// <returns>Return whether the content of an element should be translated or not</returns>
        public bool IsTranslate() { return HaveAttribute("translate"); }
    }
}
