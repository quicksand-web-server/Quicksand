using Quicksand.Web.Html;
using System.Text;
using System.Text.Json.Nodes;

namespace Quicksand.Web
{
    /// <summary>
    /// HTML document model use by <seealso cref="Controler"/>
    /// </summary>
    public class Model : ElementBase
    {
        private Head m_Head;
        private Body m_Body = new();
        private readonly List<JsonObject> m_Requests = new();
        private readonly List<Widget> m_Widgets = new();
        private readonly object m_RequestListLock = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the html document</param>
        public Model(string title) : base("html")
        {
            SetListener(this);
            AddElementToValidationList(new() { "head", "body" });
            m_Head = new(title);
            AddChild(m_Head);
            AddChild(m_Body);
        }

        internal Model(Head head, Body body, List<Widget> widgets): base("html")
        {
            SetListener(this);
            AddElementToValidationList(new() { "head", "body" });
            m_Head = head;
            m_Body = body;
            m_Widgets = widgets;
            AddChild(m_Head);
            AddChild(m_Body);
        }

        internal Model(): base("html")
        {
            SetListener(this);
            AddElementToValidationList(new() { "head", "body" });
            m_Head = new(); //This assignation will be overwriten with OnDuplicateElementAdded
        }

        /// <returns>A new <seealso cref="ElementBase"/> of the same type</returns>
        protected override ElementBase MakeDuplicate() { return new Model(); }

        /// <summary>
        /// Function called when a child is added to a duplicate
        /// </summary>
        /// <param name="original">Duplicated element</param>
        /// <param name="child">Child added</param>
        protected override void OnDuplicateElementAdded(ElementBase original, ElementBase child)
        {
            if (child is Head head)
                m_Head = head;
            else if (child is Body body)
                m_Body = body;
            foreach (Widget widget in ((Model)original).m_Widgets)
            {
                if (widget.GetElementID() == child["id"])
                {
                    Widget? newWidget = ModelParser.BuildWidget(widget.GetName(), widget.GetAttributes());
                    if (newWidget != null)
                    {
                        newWidget.SetElement((Element)child);
                        m_Widgets.Add(newWidget);
                    }
                    return;
                }
            }
        }

        internal string? Update(long deltaTime)
        {
            foreach (Widget widget in m_Widgets)
                widget.Update(deltaTime);
            lock (m_RequestListLock)
            {
                if (m_Requests.Count > 0)
                {
                    JsonObject requests = new();
                    JsonArray requestArray = new();
                    foreach (JsonObject request in m_Requests)
                        requestArray.Add(request);
                    requests["requests"] = requestArray;
                    m_Requests.Clear();
                    return requests.ToJsonString();
                }
            }
            return null;
        }

        /// <returns>The head <seealso cref="ElementBase"/> of the document</returns>
        public Head GetHead() { return m_Head; }

        /// <returns>The body <seealso cref="ElementBase"/> of the document</returns>
        public Body GetBody() { return m_Body; }

        internal void RegisterWidget(Widget widget) { m_Widgets.Add(widget); }

        /// <returns>A string containing the HTML of the document</returns>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine("<!DOCTYPE html>");
            Append(ref builder, 0);
            return builder.ToString();
        }

        internal void OnContentAdded(ElementBase element, string content, int pos)
        {
            lock (m_RequestListLock)
            {
                m_Requests.Add(new()
                {
                    ["name"] = "content-added",
                    ["parentID"] = element["id"],
                    ["content"] = content,
                    ["position"] = pos
                });
            }
        }

        internal void OnChildAdded(ElementBase element, ElementBase child, int pos)
        {
            if (child is ElementBase.Placeholder placeholder)
            {
                OnContentAdded(element, placeholder.Content, pos);
                return;
            }

            Dictionary<string, string> attributes = child.GetAttributes();
            JsonArray attributeArray = new();
            foreach (var attr in attributes)
            {
                attributeArray.Add(new JsonObject()
                {
                    ["key"] = attr.Key,
                    ["value"] = attr.Value
                });
            }

            lock (m_RequestListLock)
            {
                m_Requests.Add(new()
                {
                    ["name"] = "child-added",
                    ["parentID"] = element["id"],
                    ["childType"] = child.GetName(),
                    ["childAttributes"] = attributeArray,
                    ["position"] = pos
                });
            }

            List<ElementBase> children = child.GetChildren();
            int childPos = 0;
            foreach (ElementBase newChild in children)
            {
                OnChildAdded(child, newChild, childPos);
                ++childPos;
            }
        }

        internal void OnChildRemoved(ElementBase element, int pos)
        {
            lock (m_RequestListLock)
            {
                m_Requests.Add(new()
                {
                    ["name"] = "child-removed",
                    ["parentID"] = element["id"],
                    ["position"] = pos
                });
            }
        }

        internal void OnAttributeAdded(ElementBase element, string attribute, string value)
        {
            lock (m_RequestListLock)
            {
                m_Requests.Add(new()
                {
                    ["name"] = "attribute-added",
                    ["id"] = element["id"],
                    ["attribute"] = attribute,
                    ["value"] = value
                });
            }
        }

        internal void OnAttributeRemoved(ElementBase element, string attribute)
        {
            lock (m_RequestListLock)
            {
                m_Requests.Add(new()
                {
                    ["name"] = "attribute-removed",
                    ["id"] = element["id"],
                    ["attribute"] = attribute
                });
            }
        }
    }
}
