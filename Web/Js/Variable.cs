namespace Quicksand.Web.Js
{
    internal class Variable
    {
        private readonly string m_Name;
        private readonly bool m_IsMinifyable;

        public Variable(string name, bool isMinifyable = true)
        {
            m_Name = name;
            m_IsMinifyable = isMinifyable;
        }

        public string GetName() => m_Name;
        public bool IsMinifyable() => m_IsMinifyable;
    }
}
