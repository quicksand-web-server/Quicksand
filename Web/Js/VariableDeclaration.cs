using System.Text;

namespace Quicksand.Web.Js
{
    internal class VariableDeclaration: IInstruction
    {
        public enum Type
        {
            NONE,
            CONST,
            LET,
            VAR
        }

        private readonly bool m_IsStatic;
        private readonly Type m_Type;
        private readonly string m_Value;
        private readonly Variable m_Variable;

        public VariableDeclaration(string variableName, Type variableType, string variableValue, bool isStatic, bool isPrivate, bool minifyable)
        {
            m_IsStatic = isStatic;
            m_Type = variableType;
            m_Value = variableValue;
            if (isPrivate)
                m_Variable = new('#' + variableName, minifyable);
            else
                m_Variable = new(variableName, minifyable);
        }

        internal Variable GetVariable() => m_Variable;

        /// <summary>
        /// Append the current instruction to the <seealso cref="ScriptBuilder" />
        /// </summary>
        /// <param name="builder">ScriptBuilder to append to</param>
        internal sealed override void Append(ref ScriptBuilder builder)
        {
            StringBuilder instruction = new();
            if (m_IsStatic)
                instruction.Append("static ");
            instruction.Append(m_Type switch
            {
                Type.CONST => "const ",
                Type.LET => "let ",
                Type.VAR => "var ",
                _ => ""
            });
            instruction.Append(m_Variable.GetName());
            if (!string.IsNullOrEmpty(m_Value))
            {
                instruction.Append(" = ");
                instruction.Append(m_Value);
            }
            builder.AddInstruction(instruction.ToString());
        }
    }
}
