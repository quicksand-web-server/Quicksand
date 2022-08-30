using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quicksand.Web.Css
{
    /// <summary>
    /// Class representing a CSS HTML element selector
    /// </summary>
    public class Selector
    {
        private readonly static Regex ID_REGEX = new(@"^#[A-Za-z0-9\-_]+");
        private readonly static Regex CLASS_REGEX = new(@"^\.[A-Za-z0-9\-_]+");
        private readonly static Regex ELEMENT_REGEX = new("^[A-Za-z]+");
        private readonly static Regex ELEMENT_CLASS_REGEX = new(@"^[A-Za-z]+\.[A-Za-z]+");

        /// <summary>
        /// Parse the given string and return the <seealso cref="Selector"/> corresponding
        /// </summary>
        /// <param name="selector">String to parse</param>
        /// <returns>A corresponding <seealso cref="Selector"/>, null if none</returns>
        public static Selector? Parse(string selector)
        {
            if (selector == "*")
                return new UniversalSelector();
            else if (ID_REGEX.IsMatch(selector))
                return new IDSelector(selector[1..]);
            else if (CLASS_REGEX.IsMatch(selector))
                return new ClassSelector(selector[1..]);
            else if (ELEMENT_CLASS_REGEX.IsMatch(selector))
            {
                string[] elementClass = selector.Split('.');
                return new ElementClassSelector(elementClass[0], elementClass[1]);
            }
            else if (ELEMENT_REGEX.IsMatch(selector))
                return new ElementSelector(selector);
            return null;
        }

        private string m_Selector;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="selector">Value of the selector</param>
        protected Selector(string selector) => m_Selector = selector;
        /// <summary>
        /// Set the selector value
        /// </summary>
        /// <param name="selector">Value of the selector</param>
        protected void SetSelector(string selector) => m_Selector = selector;
        /// <returns>The value of the selector</returns>
        public string GetSelector() => m_Selector;
    }

    /// <summary>
    /// Class representing a CSS HTML element selector by ID
    /// </summary>
    public class IDSelector : Selector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">id attribute to select</param>
        public IDSelector(string id): base(string.Format("#{0}", id)) {}
        /// <summary>
        /// Set the selector value
        /// </summary>
        /// <param name="id">id attribute to select</param>
        public void SetID(string id) => SetSelector(string.Format("#{0}", id));
    }

    /// <summary>
    /// Class representing a CSS HTML element selector by class
    /// </summary>
    public class ClassSelector : Selector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="className">class attribute to select</param>
        public ClassSelector(string className) : base(string.Format(".{0}", className)) {}
        /// <summary>
        /// Set the selector value
        /// </summary>
        /// <param name="className">class attribute to select</param>
        public void SetClass(string className) => SetSelector(string.Format(".{0}", className));
    }

    /// <summary>
    /// Class representing a CSS HTML element selector by element type
    /// </summary>
    public class ElementSelector : Selector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">HTML element type to select</param>
        public ElementSelector(string element) : base(element) {}
        /// <summary>
        /// Set the selector value
        /// </summary>
        /// <param name="element">HTML element type to select</param>
        public void SetElement(string element) => SetSelector(element);
    }

    /// <summary>
    /// Class representing a CSS HTML element selector by class of element type
    /// </summary>
    public class ElementClassSelector : Selector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">HTML element type to select</param>
        /// <param name="className">class attribute to select</param>
        public ElementClassSelector(string element, string className) : base(string.Format("{0}.{1}", className, element)) {}
        /// <summary>
        /// Set the selector value
        /// </summary>
        /// <param name="element">HTML element type to select</param>
        /// <param name="className">class attribute to select</param>
        public void SetElementClass(string element, string className) => SetSelector(string.Format("{0}.{1}", className, element));
    }

    /// <summary>
    /// Class representing a CSS HTML element selector for any element
    /// </summary>
    public class UniversalSelector : Selector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UniversalSelector() : base("*") {}
    }
}
