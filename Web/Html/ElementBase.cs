using System.Text;
using System.Text.RegularExpressions;

namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing an ElementBase of an HTML document
    /// </summary>
    public abstract class ElementBase
    {
        internal class Placeholder : ElementBase
        {
            public readonly string Content;
            public Placeholder(string content): base("__qs-placeholder__") { Content = content; }
            protected override ElementBase MakeDuplicate() { return new Placeholder(Content); }
        }

        private readonly bool m_IsEmptyMarkup;
        private readonly bool m_AllowContent;
        private readonly bool m_AllowUnknownAttributes;
        private int m_ContentsCount = 0;
        private readonly string m_Name;
        private readonly Dictionary<string, string> m_Attributes = new();
        private readonly Dictionary<string, AttributeValidationRule> m_AttributesStrictValidationRules = new();
        private readonly List<Tuple<Regex, AttributeValidationRule>> m_AttributeRegexValidationRules = new();
        private readonly List<ElementBase> m_Children = new();
        private ValidationList<string> m_ElementsValidationList = new(true);
        private Model? m_ModelListener = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Type of the element</param>
        /// <param name="isEmptyMarkup">Specify if the ElementBase cannot have child/content (false by default)</param>
        /// <param name="allowContent">Specify if the ElementBase can have content (true by default)</param>
        /// <param name="allowUnknownAttributes">Specify if the ElementBase allow attributes it doesn't know (false by default)</param>
        protected ElementBase(string name, bool isEmptyMarkup = false, bool allowContent = true, bool allowUnknownAttributes = false)
        {
            m_Name = name;
            m_IsEmptyMarkup = isEmptyMarkup;
            m_AllowContent = allowContent;
            m_AllowUnknownAttributes = allowUnknownAttributes;
        }

        /// <summary>
        /// Create a duplicate of this element
        /// </summary>
        /// <returns>A duplicate of this element</returns>
        public ElementBase Duplicate()
        {
            ElementBase duplicate = MakeDuplicate();
            foreach (var pair in m_AttributesStrictValidationRules)
                duplicate.m_AttributesStrictValidationRules[pair.Key] = pair.Value;
            foreach (var tuple in m_AttributeRegexValidationRules)
                duplicate.m_AttributeRegexValidationRules.Add(new(tuple.Item1, tuple.Item2));
            foreach (var attribute in m_Attributes)
                duplicate.m_Attributes[attribute.Key] = attribute.Value;
            duplicate.m_ContentsCount = m_ContentsCount;
            foreach (ElementBase child in m_Children)
            {
                ElementBase childDuplicate = child.Duplicate();
                duplicate.m_Children.Add(childDuplicate);
                duplicate.OnDuplicateElementAdded(this, childDuplicate);
            }
            if (m_ElementsValidationList != null)
            {
                ValidationList<string> duplicateValidationList = new(m_ElementsValidationList.IsAllowList());
                foreach (string element in m_ElementsValidationList)
                    duplicateValidationList.Add(element);
                duplicate.m_ElementsValidationList = duplicateValidationList;
            }
            duplicate.m_ModelListener = m_ModelListener;
            return duplicate;
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected abstract ElementBase MakeDuplicate();

        /// <summary>
        /// Function called when a child is added to a duplicate
        /// </summary>
        /// <param name="original">Duplicated element</param>
        /// <param name="child">Child added</param>
        protected virtual void OnDuplicateElementAdded(ElementBase original, ElementBase child) {}

        /// <summary>
        /// Allow to fetch an ElementBase by its ID
        /// </summary>
        /// <param name="id">ID of the ElementBase to search</param>
        /// <returns>The ElementBase corresponding to the given ID, null if none</returns>
        public ElementBase? GetChildByID(string id)
        {
            foreach (ElementBase child in m_Children)
            {
                if (child["id"] == id)
                    return child;
                ElementBase? ret = child.GetChildByID(id);
                if (ret != null)
                    return ret;
            }
            return null;
        }

        internal List<ElementBase> GetChildren() { return m_Children; }
        /// <returns>The name of the element</returns>
        public string GetName() { return m_Name; }
        internal Dictionary<string, string> GetAttributes() { return m_Attributes; }

        internal void SetListener(Model? listener)
        {
            m_ModelListener = listener;
            foreach (ElementBase child in m_Children)
                child.SetListener(listener);
        }

        /// <summary>
        /// Specify with ElementBase child can be added by their name
        /// </summary>
        /// <param name="validationList">The validation list of the allowed or denyed childs</param>
        protected void AddElementToValidationList(List<string> validationList)
        {
            m_ElementsValidationList.Add(validationList);
        }

        private bool CanEditAttributeRule(string attributeName)
        {
            if (m_AttributesStrictValidationRules.TryGetValue(attributeName, out var validation))
                return validation.IsEditable();
            foreach (var regexValidationRule in m_AttributeRegexValidationRules)
            {
                if (regexValidationRule.Item1.IsMatch(attributeName))
                    return regexValidationRule.Item2.IsEditable();
            }
            return true;
        }

        /// <summary>
        /// Add multiple strict matching rule
        /// </summary>
        /// <remarks>
        /// See also <seealso cref="AddStrictValidationRule(string, AttributeValidationRule)"/>
        /// </remarks>
        /// <param name="rules">Dictionary of name/rule to add</param>
        protected void AddStrictValidationRule(Dictionary<string, AttributeValidationRule> rules)
        {
            foreach (var rule in rules)
            {
                if (!CanEditAttributeRule(rule.Key))
                    return;
                m_AttributesStrictValidationRules[rule.Key] = rule.Value;
            }
        }

        /// <summary>
        /// Add multiple regex matching rule
        /// </summary>
        /// <remarks>
        /// See also <seealso cref="AddRegexValidationRule(string, AttributeValidationRule)"/>
        /// </remarks>
        /// <param name="rules">List of regex/rule to add</param>
        protected void AddRegexValidationRule(List<Tuple<string, AttributeValidationRule>> rules)
        {
            foreach (var rule in rules)
            {
                if (!CanEditAttributeRule(rule.Item1))
                    return;
                m_AttributeRegexValidationRules.Add(new(new(rule.Item1), rule.Item2));
            }
        }

        /// <summary>
        /// Add a strict matching rule to an attribute name
        /// </summary>
        /// <example>
        /// <code>AddStrictValidationRule("myattribute", new IsNonEmptyStringRule());</code>
        /// Will check everytime we want to Set the attribute "myattribute" if the given value is a non empty string
        /// </example>
        /// <param name="attributeName">Name to match strictly</param>
        /// <param name="rule">Rule to comply to</param>
        protected void AddStrictValidationRule(string attributeName, AttributeValidationRule rule)
        {
            if (!CanEditAttributeRule(attributeName))
                return;
            m_AttributesStrictValidationRules[attributeName] = rule;
        }

        /// <summary>
        /// Add a regex matching rule to an attribute name
        /// </summary>
        /// <example>
        /// <code>AddRegexValidationRule("my", new IsNonEmptyStringRule());</code>
        /// Will check everytime we want to Set an attribute starting with "my" if the given value is a non empty string
        /// </example>
        /// <param name="attributeRegex">Regex to match</param>
        /// <param name="rule">Rule to comply to</param>
        protected void AddRegexValidationRule(string attributeRegex, AttributeValidationRule rule)
        {
            if (!CanEditAttributeRule(attributeRegex))
                return;
            m_AttributeRegexValidationRules.Add(new(new(attributeRegex), rule));
        }

        private bool IsAttributeValid(string name, string value)
        {
            if (m_AttributesStrictValidationRules.TryGetValue(name, out var validation))
                return validation.IsValid(value);
            foreach (var regexValidationRule in m_AttributeRegexValidationRules)
            {
                if (regexValidationRule.Item1.IsMatch(name))
                    return regexValidationRule.Item2.IsValid(value);
            }
            return m_AllowUnknownAttributes;
        }

        /// <summary>
        /// Remove the given attribute
        /// </summary>
        /// <param name="name">Name of the attribute</param>
        /// <returns>True if the attribute has been removed</returns>
        public bool RemoveAttribute(string name)
        {
            m_ModelListener?.OnAttributeRemoved(this, name);
            return m_Attributes.Remove(name);
        }

        /// <summary>
        /// [] operator overload for the getter/setter of the attributes
        /// </summary>
        /// <param name="key">Name of the attribute</param>
        /// <returns>The value of the attribute</returns>
        public string this[string key]
        {
            get => GetAttribute(key);
            set => SetAttribute(key, value);
        }

        /// <summary>
        /// Add a dictionary of attribute/value to the element
        /// </summary>
        /// <param name="attributes">Dictionary of attribute/value pair to add</param>
        /// <returns>True if all attributes has been added</returns>
        public bool AddAttributes(Dictionary<string, string> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (IsAttributeValid(attribute.Key, attribute.Value))
                {
                    m_ModelListener?.OnAttributeAdded(this, attribute.Key, attribute.Value);
                    m_Attributes[attribute.Key] = attribute.Value;
                }
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Set the given value to the attribute
        /// </summary>
        /// <param name="name">Name of the attribute</param>
        /// <param name="value">Value to set the attribute to</param>
        /// <returns>True if the attribute has been set</returns>
        public bool SetAttribute(string name, object value)
        {
            if (value != null)
            {
                string valueStr = "";
                if (value is bool boolValue)
                {
                    if (boolValue)
                        valueStr = "";
                    else
                        return RemoveAttribute(name);
                }
                else
                {
                    string? tmp = value.ToString();
                    if (tmp != null)
                        valueStr = (string)tmp;
                }

                if (IsAttributeValid(name, valueStr))
                {
                    m_ModelListener?.OnAttributeAdded(this, name, valueStr);
                    m_Attributes[name] = valueStr;
                    return true;
                }
            }
            return false;
        }

        /// <param name="attribute">Name of the attribute</param>
        /// <returns>True if the attribute exist</returns>
        protected bool HaveAttribute(string attribute)
        {
            return m_Attributes.ContainsKey(attribute);
        }

        /// <param name="attribute">Name of the attribute</param>
        /// <returns>The value of the attribute</returns>
        protected string GetAttribute(string attribute)
        {
            if (m_Attributes.TryGetValue(attribute, out string? value))
                return value;
            return "";
        }

        /// <summary>
        /// Add a child to the element
        /// </summary>
        /// <param name="element">Child to add</param>
        /// <param name="pos">Position of the child to add</param>
        /// <returns>True if the child has been added</returns>
        public bool AddChild(ElementBase element, int pos = -1)
        {
            //Validation list emptiness checking is there for markup allowing any child.
            //If a markup is non empty and didn't specify an allow list it means it allow all element
            if (m_IsEmptyMarkup || (!m_ElementsValidationList.IsEmpty() && !m_ElementsValidationList.Validate(element.m_Name)))
                return false;
            int posToAdd = (pos == -1) ? m_Children.Count : pos;
            element.SetListener(m_ModelListener);
            m_ModelListener?.OnChildAdded(this, element, posToAdd);
            m_Children.Insert(posToAdd, element);
            return true;
        }

        /// <summary>
        /// Add a widget to the element
        /// </summary>
        /// <param name="widget">Widget to add</param>
        /// <param name="pos">Position of the ElementBase of the widget to add</param>
        /// <returns>True if the widget has been added</returns>
        public bool AddWidget(Widget widget, int pos = -1)
        {
            if (AddChild(widget.GetElement(), pos))
            {
                m_ModelListener?.RegisterWidget(widget);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the child at the given pos from the children of the element
        /// </summary>
        /// <param name="pos">Position of the child to remove</param>
        public void RemoveChild(int pos)
        {
            if (pos < m_Children.Count)
                RemoveChild(m_Children[pos]);
        }

        /// <summary>
        /// Remove the given child from the children of the element
        /// </summary>
        /// <param name="element">Child to remove</param>
        public void RemoveChild(ElementBase element)
        {
            if (element is Placeholder)
                --m_ContentsCount;
            else
                element.SetListener(null);
            m_ModelListener?.OnChildRemoved(this, m_Children.IndexOf(element));
            m_Children.Remove(element);
        }

        /// <summary>
        /// Add a &lt;br/&gt; to the child of this element
        /// </summary>
        public void AddLineBreak()
        {
            AddChild(new LineBreak());
        }

        /// <summary>
        /// Add content to the element
        /// </summary>
        /// <param name="rawElement">Raw content to add</param>
        /// <param name="pos">Position to add the content in the children list</param>
        /// <param name="rawAdd">Specify if we don't treat the content before adding it (false by default)</param>
        /// <returns></returns>
        public bool AddContent(string rawElement, int pos = -1, bool rawAdd = false)
        {
            if (m_IsEmptyMarkup || !m_AllowContent)
                return false;
            int posToAdd = (pos == -1) ? m_Children.Count : pos;
            if (rawAdd)
            {
                if (rawElement.Length > 0)
                {
                    m_ModelListener?.OnContentAdded(this, rawElement, posToAdd);
                    m_Children.Insert(posToAdd, new Placeholder(rawElement));
                    ++m_ContentsCount;
                }
            }
            else
            {
                string element = rawElement.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;").Replace("\'", "&apos;").Replace("\"", "&quot;");
                string[] lines = element.Split('\n');
                int lineIdx = 0;
                foreach (string line in lines)
                {
                    if (lineIdx != 0)
                    {
                        AddChild(new LineBreak(), posToAdd);
                        ++posToAdd;
                    }
                    string trimmedLine = line.Trim();
                    if (trimmedLine.Length > 0)
                    {
                        m_ModelListener?.OnContentAdded(this, trimmedLine, posToAdd);
                        m_Children.Insert(posToAdd, new Placeholder(trimmedLine));
                        ++m_ContentsCount;
                        ++posToAdd;
                    }
                    ++lineIdx;
                }
            }
            return true;
        }

        /// <summary>
        /// Append the content of this ElementBase to a <seealso cref="StringBuilder"/>
        /// </summary>
        /// <param name="builder"><seealso cref="StringBuilder"/> to append the content to</param>
        /// <param name="tab">Specify how many tab to add after new line</param>
        protected void Append(ref StringBuilder builder, int tab)
        {
            builder.Append('<');
            builder.Append(m_Name);
            if (m_Attributes.Count == 0 && m_Children.Count == 0)
            {
                builder.Append("/>");
                return;
            }
            foreach (KeyValuePair<string, string> attribute in m_Attributes)
            {
                if (string.IsNullOrEmpty(attribute.Value))
                {
                    builder.Append(' ');
                    builder.Append(attribute.Key);
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
            if (!m_IsEmptyMarkup)
            {
                foreach (ElementBase child in m_Children)
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
                if (m_Children.Count != 0 && (m_Children.Count != 1 || m_ContentsCount != 1))
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
