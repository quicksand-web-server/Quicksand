namespace Quicksand.Web.Html
{
    /// <summary>
    /// Class representing the &lt;title&gt; markup of an HTML document
    /// </summary>
    public class Title : Element
    {
        private readonly string m_Title;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the HTML document</param>
        public Title(string title) : base("title")
        {
            m_Title = title;
            AddContent(title);
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Title(m_Title); }
    }
}
