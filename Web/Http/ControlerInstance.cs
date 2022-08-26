namespace Quicksand.Web.Http
{
    internal class ControlerInstance : Resource
    {
        private readonly IControlerBuilder m_ControlerBuilder;

        internal ControlerInstance(DelegateControlerBuilder.Delegate controlerBuilder, bool singleInstance): base()
        {
            if (singleInstance)
                m_ControlerBuilder = new DelegateSingletonControlerBuilder(controlerBuilder);
            else
                m_ControlerBuilder = new DelegateControlerBuilder(controlerBuilder);
        }

        internal ControlerInstance(IControlerBuilder controlerBuilder): base()
        {
            m_ControlerBuilder = controlerBuilder;
        }

        protected override void Get(int clientID, Request request)
        {
            Controler? newControler = m_ControlerBuilder.Build();
            if (newControler != null)
            {
                AddControler(newControler);
                newControler.CallGet(clientID, request);
            }
        }
    }
}
