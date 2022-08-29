namespace Quicksand.Web.Js
{
    internal class NewLine : IInstruction
    {
        internal override void Append(ref ScriptBuilder builder) => builder.AddLine();
    }
}
