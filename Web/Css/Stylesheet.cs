using Quicksand.Web.Http;
using System.Text;

namespace Quicksand.Web.Css
{
    /// <summary>
    /// Class representing a CSS stylesheet
    /// </summary>
    public class Stylesheet : Resource
    {
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
    }
}
