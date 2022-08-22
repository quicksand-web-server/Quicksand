namespace Quicksand.Web.Http
{
    internal class ControlerInstance : Resource
    {
        private readonly IControlerBuilder m_ControlerBuilder;

        internal ControlerInstance(DelegateControlerBuilder.Delegate controlerBuilder, bool singleInstance)
        {
            if (singleInstance)
                m_ControlerBuilder = new DelegateSingletonControlerBuilder(controlerBuilder);
            else
                m_ControlerBuilder = new DelegateControlerBuilder(controlerBuilder);
        }

        internal ControlerInstance(IControlerBuilder controlerBuilder)
        {
            m_ControlerBuilder = controlerBuilder;
        }

        internal override sealed void OnRequest(int clientID, Request request)
        {
            Controler? newControler = m_ControlerBuilder.Build();
            if (newControler != null)
            {
                AddControler(newControler);
                newControler.OnRequest(clientID, request);
            }
        }

        protected override void Get(int clientID, Request request) {} //No need to define it as OnRequest is overriden
    }
}
