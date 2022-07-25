namespace Quicksand.Web.Html
{
    public interface IBaseElementListener
    {
        public void OnRegistered(BaseElement element);
        public void OnContentAdded(BaseElement element, string content);
        public void OnContentRemoved(BaseElement element, string content);
        public void OnChildAdded(BaseElement element, BaseElement child);
        public void OnChildRemoved(BaseElement element, BaseElement child);
        public void OnAttributeAdded(BaseElement element, string attribute, object value);
        public void OnAttributeRemoved(BaseElement element, string attribute);
        public void OnUnregistered(BaseElement element);
    }
}
