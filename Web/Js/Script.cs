using Quicksand.Web.Http;
using System.Text.RegularExpressions;

namespace Quicksand.Web.Js
{
    public class Script: Resource
    {
        internal readonly static string VARIABLE_NAME_PATTERN = "^[a-zA-Z_]+[a-zA-Z0-9_]*$";
        private readonly List<Variable> m_Variables = new();
        private readonly List<IInstruction> m_Instructions = new();
        internal Script AddVariableInstruction(VariableDeclaration instruction)
        {
            Variable variable = instruction.GetVariable();
            if (Regex.IsMatch(variable.GetName(), VARIABLE_NAME_PATTERN))
            {
                m_Variables.Add(instruction.GetVariable());
                m_Instructions.Add(instruction);
            }
            return this;
        }

        public Script Variable(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.VAR, variableValue, false, false, minifyable));

        public Script LetVariable(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.LET, variableValue, false, false, minifyable));

        public Script ConstVariable(string variableName, string variableValue = "", bool minifyable = true) =>
            AddVariableInstruction(new VariableDeclaration(variableName, VariableDeclaration.Type.CONST, variableValue, false, false, minifyable));

        public Script NewLine()
        {
            m_Instructions.Add(new NewLine());
            return this;
        }

        public Class Class(string className)
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

        public Function NewFunction(string name, List<Tuple<string, string>> parameters, bool isStatic, bool isPrivate)
        {
            Function functionToAdd = new(name, parameters, isStatic, isPrivate);
            m_Instructions.Add(functionToAdd);
            NewLine();
            return functionToAdd;
        }

        public Script Instruction(string instruction)
        {
            m_Instructions.Add(new Instruction(instruction));
            return this;
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
        private bool CanAddElse()
        {
            if (m_Instructions.Count > 0)
                return (m_Instructions[^1] is ElseIf || m_Instructions[^1] is If);
            return false;
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

        internal static void Append(ref ScriptBuilder builder, List<IInstruction> instructionsList)
        {
            List<IInstruction> instructions = new();
            instructions.AddRange(instructionsList);
            while (instructions.Count > 0 && instructions[^1] is NewLine)
                instructions.RemoveAt(instructions.Count - 1);
            foreach (IInstruction instruction in instructions)
                instruction.Append(ref builder);
        }

        public string ToMinify()
        {
            ScriptBuilder builder = new(true);
            Append(ref builder, m_Instructions);
            return builder.ToString();
        }

        public sealed override string ToString()
        {
            ScriptBuilder builder = new(false);
            Append(ref builder, m_Instructions);
            return builder.ToString();
        }

        protected override void Get(int clientID, Request request)
        {
            if (m_Instructions.Count > 0)
                SendResponse(clientID, Defines.NewResponse(200, ToMinify(), MIME.TEXT.JAVASCRIPT));
            else
                SendError(clientID, Defines.NewResponse(404));
        }
    }
}
