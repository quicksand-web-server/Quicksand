namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;div&gt; markup of an HTML document
    /// </summary>
    public class Div: Element
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Div() : base("div") { }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Div(); }
    }
}
