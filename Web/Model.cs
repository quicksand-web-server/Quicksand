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
        /// <summary>Exception thrown when <seealso cref="ModelParser"/> try to load a misformatted HTML file</summary>
        public class MisformattedHTMLException : Exception
        {
            /// <summary>Constructor</summary><param name="message">Exception message</param>
            public MisformattedHTMLException(string message) : base(message) { }
        }

        /// <summary>Exception thrown when <seealso cref="ModelParser"/> try to load a misformatted QSML file</summary>
        public class MisformattedQSMLException : Exception
        {
            /// <summary>Constructor</summary><param name="message">Exception message</param>
            public MisformattedQSMLException(string message) : base(message) { }
        }

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
                    Widget? newWidget = BuildWidget(widget.GetName(), widget.GetAttributes());
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

        /// <summary>
        /// Class that represent all the information of an element in the document
        /// </summary>
        public class ElementInfo
        {
            /// <summary>
            /// Name of the element
            /// </summary>
            public string Name = "";
            /// <summary>
            /// Attributes of the element
            /// </summary>
            public Dictionary<string, string> Attributes = new();
            /// <summary>
            /// Content of the element
            /// </summary>
            public string Content = "";
        }

        /// <summary>
        /// Delegate that create a new widget
        /// </summary>
        /// <param name="attributes">All the qs-widget-* attributes within the document</param>
        /// <returns>The newly created controler</returns>
        public delegate Widget? WidgetBuilder(Dictionary<string, string> attributes);

        private static readonly Dictionary<string, WidgetBuilder> ms_WidgetFactory = new();

        /// <summary>
        /// Register a new <seealso cref="Widget"/> to the factory
        /// </summary>
        /// <param name="type">Type of the widget stored in the attribute "qs-widget-type"</param>
        /// <param name="builder">Delegate to create a new widget</param>
        public static void RegisterWidget(string type, WidgetBuilder builder)
        {
            ms_WidgetFactory[type] = builder;
        }

        private delegate Element? GenerateElement(ElementInfo info);

        private static string ToUTF8String(string content)
        {
            return content
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&apos;", "'")
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Trim();
        }

        private static string[] QuoteSplit(string str, char delimiter)
        {
            List<string> ret = new();
            StringBuilder builder = new();
            bool isInQuote = false;
            foreach (char c in str)
            {
                if (isInQuote)
                {
                    if (c == '"')
                        isInQuote = false;
                    builder.Append(c);
                }
                else
                {
                    if (c == delimiter)
                    {
                        ret.Add(builder.ToString());
                        builder.Clear();
                    }
                    else
                    {
                        if (c == '"')
                            isInQuote = true;
                        builder.Append(c);
                    }
                }
            }
            if (builder.Length > 0)
                ret.Add(builder.ToString());
            return ret.ToArray();
        }

        private static Tuple<ElementInfo, string> NextNode(string content)
        {
            if (content[0] != '<')
                throw new MisformattedHTMLException("Missing <");
            string elementName;
            int endOfOpenMarkup = content.IndexOf('>');
            int firstSpace = content.IndexOf(' ');
            if (firstSpace >= 0 && firstSpace <= endOfOpenMarkup)
                elementName = content[1..firstSpace];
            else
                elementName = content[1..endOfOpenMarkup];
            string openMarkup = content[(elementName.Length + 1)..endOfOpenMarkup].Trim();
            string markupContent, remainingContent;
            if (!string.IsNullOrEmpty(elementName) && elementName[^1] == '/')
            {
                elementName = elementName[..^1].Trim();
                markupContent = "";
                remainingContent = content[(endOfOpenMarkup + 1)..].Trim();
            }
            else
            {
                string endMarkup = string.Format("</{0}>", elementName);
                if (content.Contains(endMarkup))
                {
                    int endMarkupIndex = content.IndexOf(endMarkup);
                    markupContent = content[(endOfOpenMarkup + 1)..endMarkupIndex].Trim();
                    remainingContent = content[(endMarkupIndex + endMarkup.Length)..].Trim();
                }
                else
                {
                    markupContent = "";
                    remainingContent = content[(endOfOpenMarkup + 1)..].Trim();

                }
            }
            ElementInfo info = new() { Name = elementName, Content = markupContent.Trim() };
            if (!string.IsNullOrWhiteSpace(openMarkup))
            {
                string[] lines = QuoteSplit(openMarkup, ' ');
                foreach (string attributeLine in lines)
                {
                    string[] attributeLines = QuoteSplit(attributeLine, '=');
                    if (attributeLines.Length == 1)
                        info.Attributes.Add(attributeLines[0], "");
                    if (attributeLines.Length != 2 || attributeLines[1][0] != '"' || attributeLines[1][^1] != '"')
                        throw new MisformattedHTMLException(string.Format("Missformatted attribute: {0}", attributeLine));
                    string name = attributeLines[0];
                    string rawValue = attributeLines[1];
                    if (rawValue.Length < 2 || rawValue[0] != '"' || rawValue[^1] != '"')
                        throw new MisformattedHTMLException(string.Format("Missformatted attribute value: {0}", rawValue));
                    info.Attributes.Add(name, ToUTF8String(rawValue[1..^1]));
                }
            }
            return new(info, remainingContent.Trim());
        }

        internal static Widget BuildWidget(string widgetType, Dictionary<string, string> widgetAttributes)
        {
            if (ms_WidgetFactory.TryGetValue(widgetType, out var builder))
            {
                Widget? newWidget = builder(widgetAttributes);
                if (newWidget != null)
                {
                    newWidget.Init(widgetType, widgetAttributes);
                    return newWidget;
                }
                throw new MisformattedQSMLException("Cannot create new widget: Builder error");
            }
            throw new MisformattedQSMLException(string.Format("Cannot create new widget: Unknown widget type {0}", widgetType));
        }

        internal static Element BuildElement(ElementInfo info)
        {
            Element element = info.Name switch
            {
                "div" => new Div(),
                "br" => new LineBreak(),
                "title" => throw new MisformattedHTMLException("title Markup isn't supported outside of head Markup"),
                "style" => throw new MisformattedHTMLException("style Markup isn't supported outside of head Markup"),
                "base" => throw new MisformattedHTMLException("base Markup isn't supported outside of head Markup"),
                "link" => throw new MisformattedHTMLException("link Markup isn't supported outside of head Markup"),
                "meta" => throw new MisformattedHTMLException("meta Markup isn't supported outside of head Markup"),
                "script" => throw new MisformattedHTMLException("script Markup isn't supported outside of head Markup"),
                _ => new Markup(info.Name)
            };
            if (!element.AddAttributes(info.Attributes))
                throw new MisformattedHTMLException(string.Format("Cannot add attributes to {0} Markup", info.Name));
            return element;
        }

        private static Widget GenerateWidget(ref ElementInfo info)
        {
            List<string> attributeToRemove = new();
            Dictionary<string, string> attributes = new();
            foreach (var pair in info.Attributes)
            {
                if (pair.Key.StartsWith("qs-widget-"))
                {
                    attributeToRemove.Add(pair.Value);
                    attributes[pair.Key["qs-widget-".Length..]] = pair.Value;
                }
            }

            foreach (string toRemove in attributeToRemove)
                info.Attributes.Remove(toRemove);

            if (attributes.TryGetValue("type", out var type))
            {
                attributes.Remove("type");
                return BuildWidget(type, attributes);
            }
            throw new MisformattedQSMLException("Cannot create new widget: No type has been provided");
        }

        private static void FillElement(ref List<Widget> widgets, ref Element element, string content, string doctype)
        {
            while (!string.IsNullOrWhiteSpace(content))
            {
                int openMarkupIdx = content.IndexOf('<');
                while (openMarkupIdx != 0)
                {
                    string elementContent;
                    if (openMarkupIdx > 0)
                    {
                        elementContent = content[..openMarkupIdx];
                        content = content[openMarkupIdx..];
                    }
                    else
                    {
                        elementContent = content;
                        content = "";
                    }
                    if (!element.AddContent(ToUTF8String(elementContent)))
                        throw new MisformattedHTMLException(string.Format("Element {0} doesn't allow content", element.GetName()));
                    if (openMarkupIdx < 0)
                        return;
                    openMarkupIdx = content.IndexOf('<');
                }
                if (!string.IsNullOrWhiteSpace(content))
                {
                    Tuple<ElementInfo, string> node = NextNode(content);
                    ElementInfo info = node.Item1;
                    if (info.Name == "qs-widget")
                    {
                        if (doctype != "qsml")
                            throw new MisformattedQSMLException("Cannot create new widget: Trying to add qs-widget in an HTML document");
                        Widget widget = GenerateWidget(ref info);
                        if (!element.AddWidget(widget))
                            throw new MisformattedHTMLException(string.Format("Element {0} doesn't allow {1} Markup", element.GetName(), widget.GetElement().GetName()));
                        widgets.Add(widget);
                        ref Element widgetElement = ref widget.GetElementRef();
                        widgetElement.AddAttributes(info.Attributes);
                        FillElement(ref widgets, ref widget.GetElementRef(), info.Content, doctype);
                        content = node.Item2;
                    }
                    else
                    {
                        Element newElement = BuildElement(node.Item1);
                        FillElement(ref widgets, ref newElement, info.Content, doctype);
                        content = node.Item2;
                        if (!element.AddChild(newElement))
                            throw new MisformattedHTMLException(string.Format("Element {0} doesn't allow {1} Markup", element.GetName(), newElement.GetName()));
                    }
                }
            }
        }

        private static bool AddBase(ref Head head, ElementInfo childInfo)
        {
            if (childInfo.Attributes.TryGetValue("target", out var target) && childInfo.Attributes.TryGetValue("href", out var href))
            {
                Base.Type type = target switch
                {
                    "_blank" => Base.Type.BLANK,
                    "_parent" => Base.Type.PARENT,
                    "_self" => Base.Type.SELF,
                    "_top" => Base.Type.TOP,
                    _ => throw new MisformattedHTMLException(string.Format("Unknown base type {0}", target)),
                };
                head.SetBase(type, href);
                return true;
            }
            throw new MisformattedHTMLException("Missing target or href attribute in base Markup");
        }

        private static bool AddMeta(ref Head head, ElementInfo childInfo)
        {
            if (childInfo.Attributes.TryGetValue("content", out var metaContent))
            {
                Meta.Type metaType;
                if (childInfo.Attributes.TryGetValue("name", out var name))
                {
                    metaType = name switch
                    {
                        "application-name" => Meta.Type.APPLICATION_NAME,
                        "author" => Meta.Type.AUTHOR,
                        "description" => Meta.Type.DESCRIPTION,
                        "generator" => Meta.Type.GENERATOR,
                        "keywords" => Meta.Type.KEYWORDS,
                        "viewport" => Meta.Type.VIEWPORT,
                        _ => throw new MisformattedHTMLException(string.Format("Unknown name type {0}", name))
                    };
                }
                else if (childInfo.Attributes.TryGetValue("http-equiv", out var httpEquiv))
                {
                    metaType = httpEquiv switch
                    {
                        "content-security-policy" => Meta.Type.CONTENT_SECURITY_POLICY,
                        "content-type" => Meta.Type.CONTENT_TYPE,
                        "default-style" => Meta.Type.DEFAULT_STYLE,
                        "refresh" => Meta.Type.REFRESH,
                        _ => throw new MisformattedHTMLException(string.Format("Unknown http-equiv type {0}", httpEquiv))
                    };
                }
                else
                    throw new MisformattedHTMLException("Missing name or http-equiv attribute in meta Markup");
                head.AddMeta(metaType, metaContent);
                return true;
            }
            else if (childInfo.Attributes.TryGetValue("charset", out var charset))
            {
                head.AddMeta(Meta.Type.CHARSET, charset);
                return true;
            }
            throw new MisformattedHTMLException("Missing content or charset attribute in meta Markup");
        }

        private static bool AddLink(ref Head head, ElementInfo childInfo)
        {
            if (childInfo.Attributes.TryGetValue("rel", out var rel))
            {
                Link link = rel switch
                {
                    "alternate" => new(Link.Type.ALTERNATE),
                    "author" => new(Link.Type.AUTHOR),
                    "dns-prefetch" => new(Link.Type.DNS_PREFETCH),
                    "help" => new(Link.Type.HELP),
                    "icon" => new(Link.Type.ICON),
                    "license" => new(Link.Type.LICENSE),
                    "next" => new(Link.Type.NEXT),
                    "pingback" => new(Link.Type.PINGBACK),
                    "preconnect" => new(Link.Type.PRECONNECT),
                    "prefetch" => new(Link.Type.PREFETCH),
                    "preload" => new(Link.Type.PRELOAD),
                    "prerender" => new(Link.Type.PRERENDER),
                    "prev" => new(Link.Type.PREV),
                    "search" => new(Link.Type.SEARCH),
                    "stylesheet" => new(Link.Type.STYLESHEET),
                    _ => throw new MisformattedHTMLException(string.Format("Unknown rel type {0}", rel)),
                };
                childInfo.Attributes.Remove("rel");
                if (!link.AddAttributes(childInfo.Attributes))
                    throw new MisformattedHTMLException("Cannot add attributes to link Markup");
                head.AddLink(link);
                return true;
            }
            throw new MisformattedHTMLException("Missing rel attribute in link Markup");
        }

        private static bool AddTitle(ref Head head, ElementInfo childInfo)
        {
            if (string.IsNullOrWhiteSpace(childInfo.Content))
                throw new MisformattedHTMLException("No title has been provided in title Markup");
            Title title = new(childInfo.Content);
            if (!title.AddAttributes(childInfo.Attributes))
                throw new MisformattedHTMLException("Cannot add attributes to title Markup");
            head.AddChild(title);
            return true;
        }

        private static bool AddScript(ref Head head, ElementInfo childInfo)
        {
            Script script = new();
            if (!script.AddAttributes(childInfo.Attributes))
                throw new MisformattedHTMLException("Cannot add attributes to script Markup");
            if (!string.IsNullOrWhiteSpace(childInfo.Content))
                script.SetScriptContent(childInfo.Content);
            head.AddScript(script);
            return true;
        }

        private static Head GetHead(ElementInfo headInfo)
        {
            Head head = new();
            string headContent = headInfo.Content;
            while (!string.IsNullOrEmpty(headContent))
            {
                Tuple<ElementInfo, string> node = NextNode(headContent);
                ElementInfo childInfo = node.Item1;
                _ = childInfo.Name switch
                {
                    "title" => AddTitle(ref head, childInfo),
                    "style" => false, //TODO Add stylesheet implementation
                    "base" => AddBase(ref head, childInfo),
                    "link" => AddLink(ref head, childInfo),
                    "meta" => AddMeta(ref head, childInfo),
                    "script" => AddScript(ref head, childInfo),
                    "noscript" => throw new NotImplementedException("noscript Markup in head Markup is not implemented yet"),
                    _ => throw new MisformattedHTMLException(string.Format("Unsupported Markup {0} in head Markup", childInfo.Name))
                };
                headContent = node.Item2;
            }
            return head;
        }

        private static string ExtractDoctype(ref string content)
        {
            if (!content.StartsWith("<!DOCTYPE "))
                throw new MisformattedHTMLException("Missing DOCTYPE Markup");
            content = content["<!DOCTYPE ".Length..].Trim();
            int endOfOpenMarkup = content.IndexOf('>');
            if (endOfOpenMarkup < 0)
                throw new MisformattedHTMLException("Missing >");
            string docType = content[0..endOfOpenMarkup];
            content = content[(endOfOpenMarkup + 1)..].Trim();
            return docType;
        }

        /// <summary>
        /// Load a model from an HTML or QSML content
        /// </summary>
        /// <param name="content">Content of the HTML or QSML</param>
        /// <returns>A model corresponding to the content. Null if an error occured</returns>
        public static Model Parse(string content)
        {
            content = content.Trim();
            string doctype = ExtractDoctype(ref content);
            if (doctype != "html" && doctype != "qsml")
                throw new MisformattedHTMLException(string.Format("Unsupported DOCTYPE: {0}", doctype));
            Tuple<ElementInfo, string> htmlNode = NextNode(content);
            if (htmlNode.Item1.Name != "html")
                throw new MisformattedHTMLException("Missing html Markup");
            if (htmlNode.Item2.Length != 0)
                throw new MisformattedHTMLException("Misformatted file: Content after html Markup");
            Tuple<ElementInfo, string> headNode = NextNode(htmlNode.Item1.Content);
            if (headNode.Item1.Name != "head")
                throw new MisformattedHTMLException("Missing head Markup");
            if (headNode.Item2.Length == 0)
                throw new MisformattedHTMLException("Misformatted file: No content after head Markup");
            Head head = GetHead(headNode.Item1);
            Tuple<ElementInfo, string> bodyNode = NextNode(headNode.Item2);
            if (bodyNode.Item1.Name != "body")
                throw new MisformattedHTMLException("Missing body Markup");
            if (bodyNode.Item2.Length != 0)
                throw new MisformattedHTMLException("Misformatted file: Content after body Markup");
            List<Widget> widgets = new();
            Element body = new Body();
            if (!body.AddAttributes(bodyNode.Item1.Attributes))
                throw new MisformattedHTMLException("Cannot add attributes to body Markup");
            FillElement(ref widgets, ref body, bodyNode.Item1.Content, doctype);
            if (doctype == "qsml")
            {
                Link favicon = new(Link.Type.ICON);
                favicon.SetMediaType("image/x-icon");
                favicon.SetHref(Controler.FAVICON);
                head.AddLink(favicon);
            }
            return new Model(head, (Body)body, widgets);
        }

        /// <summary>
        /// Load a model from an HTML or QSML file
        /// </summary>
        /// <param name="path">Path of the HTML or QSML file</param>
        /// <returns>A model corresponding to the file. Null if an error occured</returns>
        public static Model ParseFile(string path)
        {
            if (File.Exists(path))
                return Parse(File.ReadAllText(path));
            throw new FileNotFoundException();
        }
    }
}
