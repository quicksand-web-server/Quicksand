namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing a markup of an HTML document
    /// </summary>
    public class Markup: Element
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the HTML markup</param>
        public Markup(string name): base(name, allowUnknownAttributes: true) { }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Markup(GetName()); }
    }
}
