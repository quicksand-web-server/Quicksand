using Quicksand.Web.Html;

namespace Quicksand.Web
{
    /// <summary>
    /// Represent an HTML element updated through code
    /// </summary>
    public abstract class Widget
    {
        /// <summary>
        /// HTML element of the widget
        /// </summary>
        protected Element m_Element;

        private string m_Name = "";
        private Dictionary<string, string> m_Attributes = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">HTML element of the widget</param>
        protected Widget(Element element)
        {
            m_Element = element;
        }

        internal void Init(string name, Dictionary<string, string> attributes)
        {
            m_Name = name;
            m_Attributes = attributes;
        }

        internal void SetElement(Element element) { m_Element = element; }

        internal string GetName() { return m_Name; }
        internal Dictionary<string, string> GetAttributes() { return m_Attributes; }

        internal string GetElementID() { return m_Element["id"]; }

        internal Element GetElement() { return m_Element; }

        internal ref Element GetElementRef() { return ref m_Element; }

        internal void Update(long deltaTime)
        {
            OnUpdate(deltaTime);
        }

        /// <summary>
        /// Function called when the widget is updated
        /// </summary>
        /// <param name="deltaTime">Delta time in milliseconds since last update</param>
        protected abstract void OnUpdate(long deltaTime);
    }
}
