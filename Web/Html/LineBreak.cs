namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;br/&gt; markup of an HTML
    /// </summary>
    public class LineBreak : Element
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LineBreak() : base("br", isEmptyMarkup: true) { }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new LineBreak(); }
    }
}
