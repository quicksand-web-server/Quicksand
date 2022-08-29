using System.Text;
using System.Text.RegularExpressions;

namespace Quicksand.Web.Js
{
    internal class ScriptBuilder
    {
        private class Scope
        {
            private readonly StringBuilder m_Builder = new();
            private readonly bool m_Minify;
            private readonly bool m_Empty;
            private readonly List<Tuple<string, uint>> m_Variables = new();
            private Scope? m_Scope = null;
            private readonly int m_Tab;

            private string Minify(string line)
            {
                if (m_Minify)
                {
                    //TODO Improve minify of line with string in it
                    foreach (Tuple<string, uint> variable in m_Variables)
                    {
                        string minifiedName;
                        uint minifiedIdx = variable.Item2;
                        if (minifiedIdx != 0)
                        {
                            minifiedName = "";
                            while (minifiedIdx > 0)
                            {
                                uint modulo = (minifiedIdx - 1) % 26;
                                minifiedName = Convert.ToChar('A' + modulo) + minifiedName;
                                minifiedIdx = (minifiedIdx - modulo) / 26;
                            }
                            if (variable.Item1[0] == '#')
                                minifiedName = '#' + minifiedName;
                        }
                        else
                            minifiedName = variable.Item1;
                        StringBuilder builder = new();
                        string ret = line;
                        string regexValue = string.Format("(^|[^a-zA-Z0-9])({0})([^a-zA-Z0-9]|$)", variable.Item1);
                        Match match = Regex.Match(ret, regexValue);
                        while (match.Success)
                        {
                            if (match.Index != 0)
                                builder.Append(ret[..(match.Index + 1)]);
                            builder.Append(minifiedName);
                            if ((match.Index + match.Value.Length) != ret.Length)
                                builder.Append(match.Value[^1]);
                            ret = ret[(match.Index + match.Value.Length)..];
                            match = Regex.Match(ret, regexValue);
                        }
                        builder.Append(ret);
                        line = builder.ToString();
                    }
                }
                return line;
            }

            private void AppendLine(string line)
            {
                if (!m_Minify)
                {
                    m_Builder.Append('\t', m_Tab);
                    m_Builder.AppendLine(line);
                }
                else
                {
                    m_Builder.Append(' ');
                    m_Builder.Append(line);
                }
            }

            private void AppendScopeLine(string scopeType, string scopeParam)
            {
                if (string.IsNullOrWhiteSpace(scopeParam))
                    AppendLine(scopeType);
                else
                    AppendLine(string.Format("{0}({1})", scopeType, Minify(scopeParam)));
                if (!m_Empty)
                    AppendLine("{");
            }

            private void AppendInstructionLine(string instruction) => AppendLine(string.Format("{0};", Minify(instruction)));

            public Scope(string scopeType, string scopeParam, List<Variable> variables, List<Tuple<string, uint>> parentVariables, bool minify, int tab, bool empty)
            {
                m_Minify = minify;
                m_Empty = empty;
                foreach (var parentVariable in parentVariables)
                    m_Variables.Add(new(parentVariable.Item1, parentVariable.Item2));
                AddVariables(variables);
                m_Tab = tab - 1; //Assigning to -1 for the scope

                if (!string.IsNullOrEmpty(scopeType))
                    AppendScopeLine(scopeType, scopeParam);

                m_Tab = tab; //Assigning it to it's proper value
            }

            internal Scope(bool minify) : this("", "", new(), new(), minify, 0, false) {}

            public void AddVariables(List<Variable> variables)
            {
                uint minifyIdx = (uint)m_Variables.Count + 1;
                foreach (Variable variable in variables)
                {
                    if (variable.IsMinifyable())
                        m_Variables.Add(new(variable.GetName(), minifyIdx++));
                    else
                        m_Variables.Add(new(variable.GetName(), 0));
                }
            }

            public void AddLine()
            {
                if (m_Minify)
                    return;
                if (m_Scope != null)
                    m_Scope.AddLine();
                else
                    m_Builder.AppendLine();
            }

            public void AddInstruction(string instruction)
            {
                if (m_Scope != null)
                    m_Scope.AddInstruction(instruction);
                else
                    AppendInstructionLine(instruction);
            }

            public void OpenScope(string scopeType, string scopeParameters, List<Variable> variables, bool empty)
            {
                if (m_Scope == null)
                    m_Scope = new(scopeType, scopeParameters, variables, m_Variables, m_Minify, m_Tab + 1, empty);
                else
                    m_Scope.OpenScope(scopeType, scopeParameters, variables, empty);
            }

            public void CloseScope()
            {
                if (m_Scope != null && m_Scope.m_Scope != null)
                    m_Scope.CloseScope();
                else if (m_Scope != null && m_Scope.m_Scope == null)
                {
                    m_Builder.Append(m_Scope.GetScope());
                    if (!m_Scope.m_Empty)
                        AppendLine("}");
                    m_Scope = null;
                }
            }

            public string GetScope() => m_Builder.ToString();

            public List<Tuple<string, uint>> GetVariables() => m_Variables;
            public StringBuilder GetBuilder() => m_Builder;

            public sealed override string ToString()
            {
                while (m_Scope != null)
                    CloseScope();
                return m_Builder.ToString();
            }
        }

        private readonly Scope m_Scope;

        public ScriptBuilder(bool minify) => m_Scope = new(minify);

        public void ScopedInstruction(string scopeType, string scopeParameters, string instruction) => ScopedInstruction(scopeType, scopeParameters, new(), instruction);

        public void ScopedInstruction(string scopeType, string scopeParameters, List<Variable> variables, string instruction)
        {
            m_Scope.OpenScope(scopeType, scopeParameters, variables, true);
            m_Scope.AddInstruction(instruction);
            m_Scope.CloseScope();
        }

        public void OpenEmptyScope(string scopeType, string scopeParameters) => OpenEmptyScope(scopeType, scopeParameters, new());

        public void OpenEmptyScope(string scopeType, string scopeParameters, List<Variable> variables) => m_Scope.OpenScope(scopeType, scopeParameters, variables, true);

        public void OpenScope(string scopeType, string scopeParameters) => OpenScope(scopeType, scopeParameters, new());

        public void OpenScope(string scopeType, string scopeParameters, List<Variable> variables) => m_Scope.OpenScope(scopeType, scopeParameters, variables, false);

        public void AddVariables(List<Variable> variables) => m_Scope.AddVariables(variables);

        public void AddInstruction(string instruction) => m_Scope.AddInstruction(instruction);

        public void AddLine() => m_Scope.AddLine();

        public void CloseScope() => m_Scope.CloseScope();

        public sealed override string ToString() => m_Scope.ToString().Trim();
    }
}
