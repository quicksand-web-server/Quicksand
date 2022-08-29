namespace Quicksand.Web
{

    /// <summary>
    /// Class to parse an HTML or QSML file into a <seealso cref="Model"/>
    /// </summary>
    [ObsoleteAttribute("Function will be removed in v0.0.9. Please use Model static functions instead")]
    public static class ModelParser
    {
        /// <summary>
        /// Register a new <seealso cref="Widget"/> to the factory
        /// </summary>
        /// <param name="type">Type of the widget stored in the attribute "qs-widget-type"</param>
        /// <param name="builder">Delegate to create a new widget</param>
        [ObsoleteAttribute("Function will be removed in v0.0.9. Please use Model.RegisterWidget instead")]
        public static void RegisterWidget(string type, Model.WidgetBuilder builder)
        {
            Model.RegisterWidget(type, builder);
        }

        /// <summary>
        /// Load a model from an HTML or QSML content
        /// </summary>
        /// <param name="content">Content of the HTML or QSML</param>
        /// <returns>A model corresponding to the content. Null if an error occured</returns>
        [ObsoleteAttribute("Function will be removed in v0.0.9. Please use Model.Parse instead")]
        public static Model? LoadFromContent(string content)
        {
            return Model.Parse(content);
        }

        /// <summary>
        /// Load a model from an HTML or QSML file
        /// </summary>
        /// <param name="path">Path of the HTML or QSML file</param>
        /// <returns>A model corresponding to the file. Null if an error occured</returns>
        [ObsoleteAttribute("Function will be removed in v0.0.9. Please use Model.ParseFile instead")]
        public static Model? LoadFromFile(string path)
        {
            return Model.ParseFile(path);
        }
    }
}
