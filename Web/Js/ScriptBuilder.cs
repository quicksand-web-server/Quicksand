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
            private readonly Dictionary<string, uint> m_Variables = new();
            private Scope? m_Scope = null;
            private readonly int m_Tab;

            private string Minify(string line)
            {
                if (m_Minify)
                {
                    StringBuilder lineBuilder = new();
                    int n = 0;
                    char lastChar = '\0';
                    char stringOpen = '\0';
                    bool inString = false;
                    string possibleVariableName = "";
                    bool inVariableName = true;
                    foreach (char c in line)
                    {
                        if (inVariableName)
                        {
                            if (c != '-' && c != '_' && !(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9'))
                            {
                                if (m_Variables.TryGetValue(possibleVariableName, out var idx))
                                {
                                    string minifiedName;
                                    uint minifiedIdx = idx;
                                    if (minifiedIdx != 0)
                                    {
                                        minifiedName = "";
                                        while (minifiedIdx > 0)
                                        {
                                            uint modulo = (minifiedIdx - 1) % 26;
                                            minifiedName = Convert.ToChar('A' + modulo) + minifiedName;
                                            minifiedIdx = (minifiedIdx - modulo) / 26;
                                        }
                                        if (possibleVariableName[0] == '#')
                                            minifiedName = '#' + minifiedName;
                                    }
                                    else
                                        minifiedName = possibleVariableName;
                                    lineBuilder.Append(minifiedName);
                                    inVariableName = false;
                                    possibleVariableName = "";
                                }
                                else
                                {
                                    lineBuilder.Append(possibleVariableName);
                                    possibleVariableName = "";
                                    if (c == '#')
                                        possibleVariableName = "#";
                                    else if (!inString && (c == '"' || c == '`' || c == '\''))
                                    {
                                        stringOpen = c;
                                        inString = true;
                                        inVariableName = false;
                                        possibleVariableName = "";
                                    }
                                    else if (inString && c == stringOpen)
                                        inString = false;
                                }
                                if (c != '#')
                                    lineBuilder.Append(c);
                            }
                            else
                                possibleVariableName += c;
                        }
                        else if (inString)
                        {
                            if (c == stringOpen)
                            {
                                lineBuilder.Append(c);
                                inString = false;
                            }
                            else
                            {
                                if (lastChar == '$' && c == '{')
                                    inVariableName = true;
                                lineBuilder.Append(c);
                            }
                        }
                        else if (c == '"' || c == '`' || c == '\'')
                        {
                            stringOpen = c;
                            inString = true;
                            lineBuilder.Append(c);
                        }
                        else if (c != '-' && c != '_' && !(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9'))
                        {
                            if (c == '#')
                                possibleVariableName = "#";
                            else
                                lineBuilder.Append(c);
                            inVariableName = true;
                        }
                        else
                            lineBuilder.Append(c);
                        ++n;
                        lastChar = c;
                    }

                    if (inVariableName)
                    {
                        if (m_Variables.TryGetValue(possibleVariableName, out var idx))
                        {
                            string minifiedName;
                            uint minifiedIdx = idx;
                            if (minifiedIdx != 0)
                            {
                                minifiedName = "";
                                while (minifiedIdx > 0)
                                {
                                    uint modulo = (minifiedIdx - 1) % 26;
                                    minifiedName = Convert.ToChar('A' + modulo) + minifiedName;
                                    minifiedIdx = (minifiedIdx - modulo) / 26;
                                }
                                if (possibleVariableName[0] == '#')
                                    minifiedName = '#' + minifiedName;
                            }
                            else
                                minifiedName = possibleVariableName;
                            lineBuilder.Append(minifiedName);
                        }
                        else
                            lineBuilder.Append(possibleVariableName);
                    }
                    return lineBuilder.ToString();
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

            public Scope(string scopeType, string scopeParam, List<Variable> variables, Dictionary<string, uint> parentVariables, bool minify, int tab, bool empty)
            {
                m_Minify = minify;
                m_Empty = empty;
                foreach (var parentVariable in parentVariables)
                    m_Variables[parentVariable.Key] = parentVariable.Value;
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
                        m_Variables[variable.GetName()] = minifyIdx++;
                    else
                        m_Variables[variable.GetName()] = 0;
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
