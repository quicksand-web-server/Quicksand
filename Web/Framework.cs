using Quicksand.Web.Http;
using System.Text;

namespace Quicksand.Web
{
    internal class Framework: Resource
    {
        private readonly static string m_Framework = Minify(Properties.Resources.quicksand_framework);

        private static string Minify(string javascript)
        {
            StringBuilder builder = new();
            bool isInString = false;
            bool isInComment = false;
            char commentChar = '\0';
            char stringChar = '\0';
            char lastChar = '\0';
            foreach (char c in javascript)
            {
                if (isInComment)
                {
                    if ((commentChar == '/' && c == '\n') || (commentChar == '*' && lastChar == '*' && c == '/'))
                        isInComment = false;
                }
                else
                {
                    if (isInString)
                    {
                        builder.Append(c);
                        if (c == stringChar)
                            isInString = false;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        builder.Append(c);
                        isInString = true;
                        stringChar = c;
                    }
                    else if (lastChar == '/' && (c == '*' || c == '/'))
                    {
                        isInComment = true;
                        commentChar = c;
                    }
                    else
                        builder.Append(c);
                }
                lastChar = c;
            }
            string purifiedScript = builder.ToString().Trim();
            builder.Clear();
            string[] scriptLines = purifiedScript.Split('\n');
            foreach (string line in scriptLines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    char lastBuilderChar = '\0';
                    if (builder.Length != 0)
                        lastBuilderChar = builder[^1];
                    if (trimmedLine[0] == '{' ||
                        trimmedLine[0] == '}' ||
                        lastBuilderChar == '\0' ||
                        lastBuilderChar == ';' ||
                        lastBuilderChar == '{' ||
                        lastBuilderChar == '}' ||
                        lastBuilderChar == ')')
                        builder.Append(trimmedLine);
                    else
                    {
                        builder.Append(' ');
                        builder.Append(trimmedLine);
                    }
                }
            }
            return builder.ToString();
        }

        protected sealed override void Get(int clientID, Request request)
        {
            if (IsSecured(clientID))
                SendResponse(clientID, Defines.NewResponse(200, m_Framework.Replace("ws://", "wss://"), MIME.TEXT.JAVASCRIPT));
            else
                SendResponse(clientID, Defines.NewResponse(200, m_Framework, MIME.TEXT.JAVASCRIPT));
        }
    }
}
