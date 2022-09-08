using Quicksand.Web.Http;
using System.Text;
using File = System.IO.File;

namespace Quicksand.Web.Css
{
    /// <summary>
    /// Class representing a CSS stylesheet
    /// </summary>
    public class Stylesheet : Resource
    {
        /// <summary>Exception thrown when <seealso cref="ModelParser"/> try to load a misformatted HTML file</summary>
        public class MisformattedCSSException : Exception
        {
            /// <summary>Constructor</summary><param name="message">Exception message</param>
            public MisformattedCSSException(string message) : base(message) { }
        }

        private readonly List<Rule> m_Rules = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public Stylesheet(): base() {}

        /// <summary>
        /// Add a rule to the stylesheet
        /// </summary>
        /// <param name="rule">Rule to add</param>
        public void AddRule(Rule rule) => m_Rules.Add(rule);

        internal void Append(ref StringBuilder builder, int tab, bool breakLine)
        {
            int ruleIdx = 0;
            foreach (Rule rule in m_Rules)
            {
                if (breakLine && ruleIdx != 0)
                    builder.AppendLine();
                rule.Append(ref builder, tab, breakLine);
                ++ruleIdx;
            }
        }

        /// <returns>Return a string containing this stylesheet with line break</returns>
        public string ToFileString()
        {
            StringBuilder builder = new();
            Append(ref builder, 0, true);
            return builder.ToString();
        }

        /// <returns>Return a string containing this stylesheet without line break</returns>
        public string ToInnerString()
        {
            StringBuilder builder = new();
            Append(ref builder, 0, false);
            return builder.ToString();
        }

        /// <summary>
        /// Function called when a GET is requested on this stylesheet
        /// </summary>
        /// <remarks>
        /// When called it will send an HTTP 200 response with the file content. If file doesn't exist it will send a HTTP 404 error
        /// </remarks>
        /// <param name="clientID">ID of the client</param>
        /// <param name="request">HTTP request received from the client</param>
        protected sealed override void Get(int clientID, Request request)
        {
            if (m_Rules.Count > 0)
                SendResponse(clientID, Defines.NewResponse(200, ToFileString(), MIME.TEXT.CSS));
            else
                SendError(clientID, Defines.NewResponse(404));
        }

        /// <summary>
        /// Load a stylesheet from an CSS content
        /// </summary>
        /// <param name="content">Content of the CSS</param>
        /// <returns>A stylesheet corresponding to the content. Null if an error occured</returns>
        public static Stylesheet Parse(string content)
        {
            Stylesheet stylesheet = new();
            if (!content.Trim().EndsWith('}'))
                throw new MisformattedCSSException("Missing closing }");
            string[] rules = content.Trim().Split('}');
            foreach (string rule in rules)
            {
                if (!string.IsNullOrWhiteSpace(rule))
                {
                    if (!rule.Trim().EndsWith(';'))
                        throw new MisformattedCSSException("Missing property end ;");
                    int openIdx = rule.IndexOf('{');
                    if (openIdx == -1)
                        throw new MisformattedCSSException("Missing opening {");
                    Rule newRule = new(rule[..openIdx].Trim());
                    string ruleContent = rule[(openIdx + 1)..].Trim();
                    StringBuilder propertyName = new();
                    StringBuilder propertyValue = new();
                    bool inString = false;
                    char stringChar = '\0';
                    bool inName = true;
                    for (int i = 0; i < ruleContent.Length; ++i)
                    {
                        char c = ruleContent[i];
                        if (inName)
                        {
                            if ((c >= 'a' && c <= 'z') ||
                                (c >= 'A' && c <= 'Z') ||
                                (c >= '0' && c <= '9') ||
                                c == '-' || c == '_')
                                propertyName.Append(c);
                            else if (c == ':')
                            {
                                inName = false;
                                while (i < (ruleContent.Length - 1) && char.IsWhiteSpace(ruleContent, i + 1))
                                    ++i;
                            }
                            else if (c != '\n' && c != '\r' && c != ' ' && c != '\t')
                                throw new MisformattedCSSException(string.Format("Bad char '{0}' in property name", c));
                        }
                        else
                        {
                            if (inString)
                            {
                                propertyValue.Append(c);
                                if (c == stringChar)
                                    inString = false;
                            }
                            else
                            {
                                if (c == '"')
                                {
                                    propertyValue.Append(c);
                                    inString = true;
                                    stringChar = c;
                                }
                                else if (c == ';')
                                {
                                    newRule.AddProperty(propertyName.ToString(), propertyValue.ToString());
                                    propertyName.Clear();
                                    propertyValue.Clear();
                                    inString = false;
                                    stringChar = '\0';
                                    inName = true;
                                    while (i < (ruleContent.Length - 1) && char.IsWhiteSpace(ruleContent, i + 1))
                                        ++i;
                                }
                                else if (c != '\n' && c != '\r')
                                    propertyValue.Append(c);
                            }
                        }
                    }
                    stylesheet.AddRule(newRule);
                }
            }
            return stylesheet;
        }

        /// <summary>
        /// Load a model from an HTML or QSML file
        /// </summary>
        /// <param name="path">Path of the HTML or QSML file</param>
        /// <returns>A model corresponding to the file. Null if an error occured</returns>
        public static Stylesheet ParseFile(string path)
        {
            if (File.Exists(path))
                return Parse(File.ReadAllText(path));
            throw new FileNotFoundException();
        }
    }
}
