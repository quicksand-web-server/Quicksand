namespace Quicksand.Web.Js
{
    /// <summary>
    /// Interface representing a javascript instruction
    /// </summary>
    public abstract class IInstruction
    {
        internal abstract void Append(ref ScriptBuilder builder);
    }
}
