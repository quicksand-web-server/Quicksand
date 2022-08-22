namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;body&gt; markup of an HTML document
    /// </summary>
    public class Body : Element
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Body() : base("body") {}

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Body(); }
    }
}
