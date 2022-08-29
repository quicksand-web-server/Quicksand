using System.Text;

namespace Quicksand.Web.Css
{
    /// <summary>
    /// Class representing a CSS rule
    /// </summary>
    public class Rule
    {
        private readonly List<Selector> m_Selectors = new();
        private readonly Dictionary<string, string> m_Properties = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="selector">Required selector for the rule</param>
        public Rule(Selector selector) => m_Selectors.Add(selector);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="selectorStr">Required selector string for the rule</param>
        public Rule(string selectorStr)
        {
            Selector? selector = Selector.Parse(selectorStr);
            if (selector != null)
                m_Selectors.Add(selector);
            else
                throw new ArgumentException("Given string isn't a valid selector string");
        }

        /// <summary>
        /// Add a selector to the rule
        /// </summary>
        /// <param name="selector">Selector to add</param>
        public void AddSelector(Selector selector) => m_Selectors.Add(selector);

        /// <summary>
        /// Add a selector to the rule
        /// </summary>
        /// <param name="selectorStr">Selector string to add</param>
        public void AddSelector(string selectorStr)
        {
            Selector? selector = Selector.Parse(selectorStr);
            if (selector != null)
                m_Selectors.Add(selector);
        }

        /// <summary>
        /// Add a property to the rule
        /// </summary>
        /// <param name="property">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public void AddProperty(string property, string value) => m_Properties[property] = value;

        internal void Append(ref StringBuilder builder, int tab, bool breakLine)
        {
            builder.Append('\t', tab);
            int selectorIdx = 0;
            foreach (Selector selector in m_Selectors)
            {
                if (selectorIdx != 0)
                    builder.Append(", ");
                builder.Append(selector.GetSelector());
                ++selectorIdx;
            }
            if (breakLine)
            {
                builder.AppendLine();
                builder.Append('\t', tab);
                builder.Append('{');
            }
            else
                builder.Append(" {");
            foreach (var pair in m_Properties)
            {
                if (breakLine)
                {
                    builder.AppendLine();
                    builder.Append('\t', tab + 1);
                }
                else
                    builder.Append(' ');
                builder.Append(pair.Key);
                builder.Append('=');
                builder.Append(pair.Value);
                builder.Append(';');
            }
            if (m_Properties.Count > 0)
            {
                if (breakLine)
                {
                    builder.AppendLine();
                    builder.Append('\t', tab);
                }
                else
                    builder.Append(' ');

            }
            builder.Append('}');
            if (breakLine)
                builder.AppendLine();
        }
    }
}
