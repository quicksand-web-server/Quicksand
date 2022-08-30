using System.Text.RegularExpressions;

namespace Quicksand.Web.Js
{
    public class Class : IInstruction
    {
        private readonly string m_Name;
        private readonly List<Variable> m_Variables = new();
        private readonly List<IInstruction> m_Instructions = new();

        internal Class(string className) => m_Name = className;

        private Class AddVariableInstruction(VariableDeclaration instruction)
        {
            Variable variable = instruction.GetVariable();
            if (Regex.IsMatch(variable.GetName(), "^#?" + Script.VARIABLE_NAME_PATTERN[1..]))
            {
                m_Variables.Add(instruction.GetVariable());
                m_Instructions.Add(instruction);
            }
            return this;
        }

        public Class DeclarePublicStaticMember(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.NONE, variableValue, true, false, minifyable));

        public Class DeclarePrivateStaticMember(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.NONE, variableValue, true, true, minifyable));

        public Class DeclarePublicMember(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.NONE, variableValue, false, false, minifyable));

        public Class DeclarePrivateMember(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.NONE, variableValue, false, true, minifyable));

        public Class NewLine()
        {
            m_Instructions.Add(new NewLine());
            return this;
        }

        public Class NewClass(string className)
        {
            Class classToAdd = new(className);
            m_Instructions.Add(classToAdd);
            NewLine();
            return classToAdd;
        }

        public Function Function(string name, bool isStatic, bool isPrivate)
        {
            Function functionToAdd = new(name, isStatic, isPrivate);
            m_Instructions.Add(functionToAdd);
            NewLine();
            return functionToAdd;
        }

        public Function Function(string name, List<Tuple<string, string>> parameters, bool isStatic, bool isPrivate)
        {
            Function functionToAdd = new(name, parameters, isStatic, isPrivate);
            m_Instructions.Add(functionToAdd);
            NewLine();
            return functionToAdd;
        }

        internal sealed override void Append(ref ScriptBuilder builder)
        {
            builder.OpenScope(string.Format("class {0}", m_Name), "", m_Variables);
            Script.Append(ref builder, m_Instructions);
            builder.CloseScope();
        }
    }
}
