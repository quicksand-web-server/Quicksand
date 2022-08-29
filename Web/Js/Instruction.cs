namespace Quicksand.Web.Js
{
    /// <summary>
    /// Class representing a basic javascript instruction
    /// </summary>
    public class Instruction: IInstruction
    {
        private readonly string m_Instruction;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instruction">Instruction</param>
        public Instruction(string instruction) => m_Instruction = instruction;

        internal sealed override void Append(ref ScriptBuilder builder) => builder.AddInstruction(m_Instruction);
    }
}
