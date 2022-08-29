namespace Quicksand.Web.Js
{
    public class While : InstructionContainer<While>
    {
        internal While(string condition) : base("while ", condition, true) {}
    }

    public class If : InstructionContainer<If>
    {
        internal If(string condition) : base("if ", condition, true) {}
    }

    public class ElseIf : InstructionContainer<ElseIf>
    {
        internal ElseIf(string condition) : base("else if ", condition, true) {}
    }

    public class Else : InstructionContainer<Else>
    {
        internal Else() : base("else", true) {}
    }

    public class For : InstructionContainer<For>
    {
        internal For(string condition, string incrementation = "", string variableDeclaration = "") : base("for ", string.Format("{0}; {1}; {2}", variableDeclaration, condition, incrementation), true) {}
    }
}
