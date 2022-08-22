using System.Globalization;
using System.Text.RegularExpressions;

namespace Quicksand.Web.Html
{
    /// <summary>
    /// Rule use to check if the value given to an attribute is valid or not
    /// </summary>
    public abstract class AttributeValidationRule
    {
        private readonly bool m_IsEditable = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isEditable">Specify if this rule can be edited in the <seealso cref="ElementBase"/></param>
        protected AttributeValidationRule(bool isEditable) { m_IsEditable = isEditable; }

        /// <returns>True if this attribute validation rule can be edited in the <seealso cref="ElementBase"/></returns>
        public bool IsEditable() { return m_IsEditable; }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value follow the rule</returns>
        public abstract bool IsValid(string value);
    }

    /// <summary>
    /// Rule to check if the attribute value is a boolean string (empty is true)
    /// </summary>
    public class IsBool : AttributeValidationRule
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsBool(bool isEditable = false) : base(isEditable) { }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is an empty string (true in HTML attributes)</returns>
        public override bool IsValid(string value) { return value.Length == 0; }
    }

    /// <summary>
    /// Rule to check if the attribute value is a char
    /// </summary>
    public class IsChar: AttributeValidationRule
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsChar(bool isEditable = false) : base(isEditable) { }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is composed of one character</returns>
        public override bool IsValid(string value) { return value.Length == 1; }
    }

    /// <summary>
    /// Rule to check if the attribute value is a integer
    /// </summary>
    public class IsInt : AttributeValidationRule
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsInt(bool isEditable = false) : base(isEditable) { }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is composed of one character</returns>
        public override bool IsValid(string value) { return int.TryParse(value, out var _); }
    }

    /// <summary>
    /// Rule to check if the attribute value is a non empty string
    /// </summary>
    public class IsNonEmptyStringRule : AttributeValidationRule
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsNonEmptyStringRule(bool isEditable = false) : base(isEditable) { }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is a non empty string</returns>
        public override bool IsValid(string value){ return !string.IsNullOrWhiteSpace(value); }
    }

    /// <summary>
    /// Rule to check if the attribute value is a non empty string without space
    /// </summary>
    public class IsNonEmptyWordRule : AttributeValidationRule
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsNonEmptyWordRule(bool isEditable = false) : base(isEditable) { }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is a non empty string without space</returns>
        public override bool IsValid(string value) { return !string.IsNullOrWhiteSpace(value) && !value.Contains(' '); }
    }

    /// <summary>
    /// Rule to check if the attribute value is a valid lang
    /// </summary>
    public class IsLangRule: AttributeValidationRule
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsLangRule(bool isEditable = false) : base(isEditable) {}

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is a valid lang info</returns>
        public override bool IsValid(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    RegionInfo info = new(value);
                    return true;
                }
                catch { }
            }
            return false;
        }
    }

    /// <summary>
    /// Rule to check if the attribute value match a regex
    /// </summary>
    public class IsMatchingRegexRule : AttributeValidationRule
    {
        private readonly Regex m_Regex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="regex">Regex to match</param>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsMatchingRegexRule(string regex, bool isEditable = false) : base(isEditable) { m_Regex = new(regex); }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is a string that match the regex</returns>
        public override bool IsValid(string value) { return m_Regex.IsMatch(value); }
    }

    /// <summary>
    /// Rule to check if the attribute value is in a list
    /// </summary>
    public class IsInListRule : AttributeValidationRule
    {
        private readonly List<string> m_List;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="list">List of allowed values</param>
        /// <param name="isEditable">Specify if this rule can be edited (false by default)</param>
        public IsInListRule(List<string> list, bool isEditable = false) : base(isEditable) { m_List = list; }

        /// <param name="value">Value of the attribute to check</param>
        /// <returns>True if the value is in the given list</returns>
        public override bool IsValid(string value) { return m_List.Contains(value); }
    }

    internal class EventAttributeRule : AttributeValidationRule
    {
        private readonly bool m_IsEventAllowed = false;
        public EventAttributeRule(bool isEventAllowed): base(false) { m_IsEventAllowed = isEventAllowed; }
        public override bool IsValid(string value) { return m_IsEventAllowed && !string.IsNullOrWhiteSpace(value); }
    }

    internal class DataAttributeRule : AttributeValidationRule
    {
        public DataAttributeRule(): base(false) {}
        public override bool IsValid(string value) { return true; }
    }
}
