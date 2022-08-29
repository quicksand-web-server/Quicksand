using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace Quicksand.Web.Js
{
    public abstract class InstructionContainer<CRTP>: IInstruction where CRTP : InstructionContainer<CRTP>
    {
        private readonly List<Variable> m_Variables = new();
        private readonly List<IInstruction> m_Instructions = new();
        private readonly bool m_CanBeEmpty;
        private readonly string m_Type;
        private string m_Parameters;

        internal InstructionContainer(string type, bool canBeEmpty) : this(type, "", canBeEmpty) { }
        internal InstructionContainer(string type, string parameters, bool canBeEmpty)
        {
            m_Type = type;
            m_Parameters = parameters;
            m_CanBeEmpty = canBeEmpty;
        }

        internal void SetParameters(string parameters) => m_Parameters = parameters;

        internal sealed override void Append(ref ScriptBuilder builder)
        {
            if (m_CanBeEmpty && GetInstructionsCount() == 1)
                builder.OpenEmptyScope(m_Type, m_Parameters, GetVariables());
            else
                builder.OpenScope(m_Type, m_Parameters, GetVariables());
            Script.Append(ref builder, m_Instructions);
            builder.CloseScope();
        }

        internal List<Variable> GetVariables() => m_Variables;
        internal int GetInstructionsCount() => m_Instructions.Count;

        internal bool AddVariable(string variableName)
        {
            if (Regex.IsMatch(variableName, "^[a-zA-Z0-9]*$"))
            {
                m_Variables.Add(new(variableName));
                return true;
            }
            return false;
        }

        internal CRTP AddVariableInstruction(VariableDeclaration instruction)
        {
            Variable variable = instruction.GetVariable();
            if (Regex.IsMatch(variable.GetName(), "^[a-zA-Z0-9]*$"))
            {
                m_Variables.Add(instruction.GetVariable());
                m_Instructions.Add(instruction);
            }
            return (CRTP)this;
        }

        public CRTP Variable(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.VAR, variableValue, false, false, minifyable));

        public CRTP LetVariable(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.LET, variableValue, false, false, minifyable));

        public CRTP ConstVariable(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.CONST, variableValue, false, false, minifyable));

        private bool CanAddElse()
        {
            if (m_Instructions.Count > 0)
                return (m_Instructions[^1] is ElseIf || m_Instructions[^1] is If);
            return false;
        }

        public CRTP NewLine()
        {
            m_Instructions.Add(new NewLine());
            return (CRTP)this;
        }

        public CRTP Instruction(string instruction)
        {
            m_Instructions.Add(new Instruction(instruction));
            return (CRTP)this;
        }

        public While While(string condition)
        {
            While whileToAdd = new(condition);
            m_Instructions.Add(whileToAdd);
            return whileToAdd;
        }

        public For For(string condition, string incrementation = "", string variableDeclaration = "")
        {
            For forToAdd = new(condition, incrementation, variableDeclaration);
            m_Instructions.Add(forToAdd);
            return forToAdd;
        }

        public If If(string condition)
        {
            If ifToAdd = new(condition);
            m_Instructions.Add(ifToAdd);
            return ifToAdd;
        }

        public ElseIf ElseIf(string condition)
        {
            ElseIf elseIfToAdd = new(condition);
            if (CanAddElse())
                m_Instructions.Add(elseIfToAdd);
            return elseIfToAdd;
        }

        public Else Else()
        {
            Else elseToAdd = new();
            if (CanAddElse())
                m_Instructions.Add(elseToAdd);
            return elseToAdd;
        }
    }
}
