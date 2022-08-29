using System.Text;

namespace Quicksand.Web.Js
{
    public class Function: InstructionContainer<Function>
    {
        private static string GetFunctionName(string name, bool isStatic, bool isPrivate)
        {
            string functionName = name;
            if (isPrivate)
                functionName = '#' + functionName;
            if (isStatic)
                return "static " + functionName;
            return functionName;
        }

        internal Function(string name, bool isStatic, bool isPrivate) : base(GetFunctionName(name, isStatic, isPrivate), false) { }

        internal Function(string name, List<Tuple<string, string>> parameters, bool isStatic, bool isPrivate) : base(GetFunctionName(name, isStatic, isPrivate), false)
        {
            StringBuilder builder = new();
            foreach (Tuple<string, string> parameter in parameters)
            {
                if (AddVariable(parameter.Item1))
                {
                    if (builder.Length > 0)
                        builder.Append(", ");
                    builder.Append(parameter.Item1);
                    if (!string.IsNullOrEmpty(parameter.Item2))
                    {
                        builder.Append(" = ");
                        builder.Append(parameter.Item2);
                    }
                }
            }
            SetParameters(builder.ToString());
        }
    }
}
