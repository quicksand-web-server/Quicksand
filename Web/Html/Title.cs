namespace Quicksand.Web.Html
{
    public class Title : BaseElement
    {
        public Title(string title) : base("title")
        {
            AddContent(title);
        }
    }
}
