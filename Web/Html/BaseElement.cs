using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Quicksand.Web.Html
{
    public abstract class BaseElement
    {
        private class Placeholder : BaseElement
        {
            public readonly string Content;
            public Placeholder(string content): base("__placeholder__") { Content = content; }
        }

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

        private readonly string m_Name;
        private readonly Dictionary<string, AttributeValidationRule> m_AttributesStrictValidationRules = new()
        {
            {"accesskey", new IsTypeRule<char>()}, //Specifies a shortcut key to activate/focus an element
            {"class", new IsMatchingRegexRule("^[a-zA-Z]([a-zA-Z-_])*")}, //Specifies one or more classnames for an element(refers to a class in a style sheet)
            {"contenteditable", new IsTypeRule<bool>()}, //Specifies whether the content of an element is editable or not
            {"dir", new IsInListRule<string>(new(){"ltr", "rtl", "auto"})}, //Specifies the text direction for the content in an element
            {"draggable", new IsTypeRule<bool>()}, //Specifies whether an element is draggable or not
            {"hidden", new IsTypeRule<bool>()}, //Specifies that an element is not yet, or is no longer, relevant
            {"id", new CallbackRule(value => value is string str && !string.IsNullOrWhiteSpace(str) && !str.Contains(' '))}, //Specifies a unique id for an element
            {"lang", new IsLangRule()}, //Specifies the language of the element's content
            {"spellcheck", new IsTypeRule<bool>()}, //Specifies whether the element is to have its spelling and grammar checked or not
            {"style", new IsNonEmptyStringRule()}, //Specifies an inline CSS style for an element
            {"tabindex", new IsTypeRule<int>()}, //Specifies a shortcut key to activate/focus an element
            {"title", new IsTypeRule<string>()}, //Specifies a shortcut key to activate/focus an element
            {"translate", new IsTypeRule<bool>()} //Specifies whether the content of an element should be translated or not
        };
        private readonly Regex m_GlobalAttributeDataRegex = new("data-.*"); //Used to store custom data private to the page or application
        private readonly List<Tuple<Regex, AttributeValidationRule>> m_AttributeRegexValidationRules = new();
        private readonly bool m_IsEmptyTag;
        private readonly bool m_AllowContent;
        private readonly bool m_AllowUnknownAttributes;
        private readonly Dictionary<string, object> m_Attributes = new();
        private int m_ContentsCount = 0;
        private readonly List<BaseElement> m_Childs = new();
        private ValidationList<string>? m_ElementsValidationList = null;
        private IBaseElementListener? m_BaseElementListener = null;

        protected BaseElement(string name, bool isEmptyTag = false, bool allowContent = true, bool allowUnknownAttributes = false)
        {
            m_Name = name;
            m_IsEmptyTag = isEmptyTag;
            m_AllowContent = allowContent;
            m_AllowUnknownAttributes = allowUnknownAttributes;

            foreach (var eventRule in ms_EventsRules)
            {
                bool isAllowed = eventRule.Item2.Validate(m_Name);
                m_AttributesStrictValidationRules[eventRule.Item1] = new EventAttributeRule(isAllowed);
                if (isAllowed && eventRule.Item1 == "oncontextmenu")
                    m_AttributesStrictValidationRules["contextmenu"] = new IsNonEmptyStringRule();
            }
        }

        public void SetListener(IBaseElementListener? listener)
        {
            m_BaseElementListener?.OnUnregistered(this);
            m_BaseElementListener = listener;
            m_BaseElementListener?.OnRegistered(this);
        }

        protected void SetElementValidationList(ValidationList<string> validationList)
        {
            m_ElementsValidationList = validationList;
        }

        private bool CanEditAttributeRule(string attributeName)
        {
            if (m_AttributesStrictValidationRules.TryGetValue(attributeName, out var validation))
                return validation.IsEditable();
            else if (m_GlobalAttributeDataRegex.IsMatch(attributeName))
                return false;
            foreach (var regexValidationRule in m_AttributeRegexValidationRules)
            {
                if (regexValidationRule.Item1.IsMatch(attributeName))
                    return regexValidationRule.Item2.IsEditable();
            }
            return true;
        }

        protected void AddStrictValidationRule(Dictionary<string, AttributeValidationRule> rules)
        {
            foreach (var rule in rules)
            {
                if (!CanEditAttributeRule(rule.Key))
                    return;
                m_AttributesStrictValidationRules[rule.Key] = rule.Value;
            }
        }

        protected void AddRegexValidationRule(List<Tuple<string, AttributeValidationRule>> rules)
        {
            foreach (var rule in rules)
            {
                if (!CanEditAttributeRule(rule.Item1))
                    return;
                m_AttributeRegexValidationRules.Add(new(new(rule.Item1), rule.Item2));
            }
        }

        protected void AddStrictValidationRule(string attributeName, AttributeValidationRule rule)
        {
            if (!CanEditAttributeRule(attributeName))
                return;
            m_AttributesStrictValidationRules[attributeName] = rule;
        }

        protected void AddRegexValidationRule(string attributeRegex, AttributeValidationRule rule)
        {
            if (!CanEditAttributeRule(attributeRegex))
                return;
            m_AttributeRegexValidationRules.Add(new(new(attributeRegex), rule));
        }

        private bool IsAttributeValid(string name, object value)
        {
            if (m_GlobalAttributeDataRegex.IsMatch(name))
                return true;
            else if (m_AttributesStrictValidationRules.TryGetValue(name, out var validation))
                return validation.IsValid(value);
            foreach (var regexValidationRule in m_AttributeRegexValidationRules)
            {
                if (regexValidationRule.Item1.IsMatch(name))
                    return regexValidationRule.Item2.IsValid(value);
            }
            return m_AllowUnknownAttributes;
        }

        public bool RemoveAttribute(string name)
        {
            m_BaseElementListener?.OnAttributeRemoved(this, name);
            return m_Attributes.Remove(name);
        }

        public object this[string key]
        {
            get => m_Attributes[key];
            set => AddAttribute(key, value);
        }

        public bool AddAttribute(string name, object value)
        {
            if (IsAttributeValid(name, value))
            {
                m_BaseElementListener?.OnAttributeAdded(this, name, value);
                m_Attributes[name] = value;
                return true;
            }
            return false;
        }

        private T GetAttribute<T>(string attribute, T defaultVal)
        {
            if (m_Attributes.TryGetValue(attribute, out object? value))
                return (T)value;
            return defaultVal;
        }

        public void SetAccessKey(char value) { _ = AddAttribute("accesskey", value); }
        public char GetAccessKey() { return GetAttribute("accesskey", '\0'); }
        public void SetClass(string value) { _ = AddAttribute("class", value); }
        public string GetClass() { return GetAttribute("class", ""); }
        public void SetContentEditable(bool value) { _ = AddAttribute("contenteditable", value); }
        public bool IsContentEditable() { return GetAttribute("contenteditable", false); }
        public void SetData(string name, object value) { _ = AddAttribute(string.Format("data-{0}", name), value); }
        public object? GetData(string name) { return GetAttribute<object?>(string.Format("data-{0}", name), null); }
        public void SetDir(string value) { _ = AddAttribute("dir", value); }
        public string GetDir() { return GetAttribute("dir", ""); }
        public void SetDraggable(bool value) { _ = AddAttribute("draggable", value); }
        public bool IsDraggable() { return GetAttribute("draggable", false); }
        public void SetHidden(bool value) { _ = AddAttribute("hidden", value); }
        public bool IsHidden() { return GetAttribute("hidden", false); }
        public void SetID(string value) { _ = AddAttribute("id", value); }
        public string GetID() { return GetAttribute("id", ""); }
        public void SetLang(string value) { _ = AddAttribute("lang", value); }
        public string GetLang() { return GetAttribute("lang", ""); }
        public void SetSpellcheck(bool value) { _ = AddAttribute("spellcheck", value); }
        public bool IsSpellcheck() { return GetAttribute("spellcheck", false); }
        public void SetStyle(string value) { _ = AddAttribute("style", value); }
        public string GetStyle() { return GetAttribute("style", ""); }
        public void SetTabIndex(int value) { _ = AddAttribute("tabindex", value); }
        public int GetTabIndex() { return GetAttribute("tabindex", -1); }
        public void SetTitle(string value) { _ = AddAttribute("title", value); }
        public string GetTitle() { return GetAttribute("title", ""); }
        public void SetTranslate(bool value) { _ = AddAttribute("translate", value); }
        public bool IsTranslate() { return GetAttribute("translate", false); }

        public void AddChild(BaseElement element)
        {
            if (m_IsEmptyTag || (m_ElementsValidationList != null && !m_ElementsValidationList.Validate(element.m_Name)))
                return;
            m_BaseElementListener?.OnChildAdded(this, element);
            m_Childs.Add(element);
        }

        public void RemoveChild(BaseElement element)
        {
            if (element is Placeholder placeholder)
            {
                m_BaseElementListener?.OnContentRemoved(this, placeholder.Content);
                --m_ContentsCount;
            }
            else
                m_BaseElementListener?.OnChildRemoved(this, element);
            m_Childs.Remove(element);
        }

        public void AddLineBreak()
        {
            AddChild(new LineBreak());
        }

        public void AddContent(string element)
        {
            if (m_IsEmptyTag || !m_AllowContent)
                return;
            string[] lines = element.Split('\n');
            int lineIdx = 0;
            foreach (string line in lines)
            {
                if (lineIdx != 0)
                    AddChild(new LineBreak());
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    m_BaseElementListener?.OnContentAdded(this, trimmedLine);
                    m_Childs.Add(new Placeholder(trimmedLine));
                    ++m_ContentsCount;
                }
                ++lineIdx;
            }
        }

        protected void Append(ref StringBuilder builder, int tab)
        {
            builder.Append('<');
            builder.Append(m_Name);
            if (m_Attributes.Count == 0 && m_Childs.Count == 0)
            {
                builder.Append("/>");
                return;
            }
            foreach (KeyValuePair<string, object> attribute in m_Attributes)
            {
                if (attribute.Value is bool value)
                {
                    if (value)
                    {
                        builder.Append(' ');
                        builder.Append(attribute.Key);
                    }
                }
                else
                {
                    builder.Append(' ');
                    builder.Append(attribute.Key);
                    builder.Append("=\"");
                    builder.Append(attribute.Value);
                    builder.Append('"');
                }
            }
            builder.Append('>');
            if (!m_IsEmptyTag)
            {
                foreach (BaseElement child in m_Childs)
                {
                    if (child is Placeholder placeholder)
                    {
                        if (m_ContentsCount > 1)
                        {
                            builder.AppendLine();
                            builder.Append('\t', tab + 1);
                        }
                        builder.Append(placeholder.Content);
                    }
                    else
                    {
                        builder.AppendLine();
                        builder.Append('\t', tab + 1);
                        child.Append(ref builder, tab + 1);
                    }
                }
                if (m_Childs.Count != 0 && (m_Childs.Count != 1 || m_ContentsCount != 1))
                {
                    builder.AppendLine();
                    builder.Append('\t', tab);
                }
                builder.Append("</");
                builder.Append(m_Name);
                builder.Append('>');
            }
        }
    }
}
