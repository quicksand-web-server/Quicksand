using System.Globalization;
using System.Text.RegularExpressions;

namespace Quicksand.Web.Html
{
    public abstract class AttributeValidationRule
    {
        protected readonly bool m_IsEditable = false;

        public AttributeValidationRule(bool isEditable)
        {
            m_IsEditable = isEditable;
        }

        public bool IsEditable() { return m_IsEditable; }
        public abstract bool IsValid(object value);
    }

    public class CallbackRule: AttributeValidationRule
    {
        public delegate bool Callback(object value);
        private readonly Callback m_Callback;

        public CallbackRule(Callback callback, bool isEditable = false): base(isEditable)
        {
            m_Callback = callback;
        }

        public override bool IsValid(object value) { return m_Callback(value); }
    }

    public class IsTypeRule<T>: AttributeValidationRule
    {
        public IsTypeRule(bool isEditable = false): base(isEditable) { }
        public override bool IsValid(object value) { return value is T; }
    }

    public class IsNonEmptyStringRule : AttributeValidationRule
    {
        public IsNonEmptyStringRule(bool isEditable = false) : base(isEditable) { }
        public override bool IsValid(object value){ return value is string str && !string.IsNullOrWhiteSpace(str); }
    }

    public class IsLangRule: AttributeValidationRule
    {
        public IsLangRule(bool isEditable = false) : base(isEditable) { }

        public override bool IsValid(object value)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                try
                {
                    RegionInfo info = new(str);
                    return true;
                }
                catch { }
            }
            return false;
        }
    }

    public class IsMatchingRegexRule : AttributeValidationRule
    {
        private readonly Regex m_Regex;

        public IsMatchingRegexRule(string regex, bool isEditable = false) : base(isEditable)
        {
            m_Regex = new(regex);
        }

        public override bool IsValid(object value)
        {
            return value is string str && m_Regex.IsMatch(str);
        }
    }

    public class IsInListRule<T> : AttributeValidationRule
    {
        private readonly List<T> m_List;

        public IsInListRule(List<T> list, bool isEditable = false) : base(isEditable)
        {
            m_List = list;
        }

        public override bool IsValid(object value)
        {
            return value is T obj && m_List.Contains(obj);
        }
    }

    public class EventAttributeRule : AttributeValidationRule
    {
        private readonly bool m_IsEventAllowed = false;
        public EventAttributeRule(bool isEventAllowed): base(false)
        {
            m_IsEventAllowed = isEventAllowed;
        }

        public override bool IsValid(object value)
        {
            if (m_IsEventAllowed)
                return value is string str && !string.IsNullOrWhiteSpace(str);
            return false;
        }
    }
}
