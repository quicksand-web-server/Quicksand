namespace Quicksand.Web
{
    public abstract class CodeDrivenHtmlResource : Resource
    {
        protected Html.Document m_Page = new("");

        public CodeDrivenHtmlResource(): base() { }
        protected CodeDrivenHtmlResource(bool needUpdate) : base(needUpdate) { }

        protected override void OnInit(params dynamic[] args)
        {
            if (args.Length >= 1)
            {
                m_Page = new(args[0]);
                Generate();
            }
        }

        protected override void Get(int clientID, Http.Request request)
        {
            if (m_Page != null)
                SendResponse(clientID, Http.Defines.NewResponse(request.Version, 200, m_Page.ToString()));
        }

        protected abstract void Generate();
    }
}
