using System.Collections;

namespace Quicksand.Web.Html
{
    /// <summary>
    /// A List that work either as an allow list or a deny list
    /// </summary>
    /// <typeparam name="T">The type contains in the list</typeparam>
    public class ValidationList<T>: IEnumerable, IEnumerator
    {
        private readonly bool m_IsAllowed;
        private readonly List<T> m_List = new();
        private int m_EnumeratorPosition = -1;

        /// <summary>
        /// Contrustor
        /// </summary>
        /// <param name="isAllowed">Specify if the list is an allow list</param>
        public ValidationList(bool isAllowed)
        {
            m_IsAllowed = isAllowed;
        }

        /// <summary>
        /// Specify if this validation list is an allow list or a deny list
        /// </summary>
        /// <returns>True is this list is an allow list</returns>
        public bool IsAllowList() { return m_IsAllowed; }

        /// <summary>
        /// Specify if this validation list is empty or not
        /// </summary>
        /// <returns>True if the list is empty</returns>
        public bool IsEmpty() { return m_List.Count == 0; }

        /// <summary>
        /// Add an element to the list
        /// </summary>
        /// <param name="elem">Element to add</param>
        public void Add(T elem)
        {
            if (!m_List.Contains(elem))
                m_List.Add(elem);
        }

        /// <summary>
        /// Append a list of element to the list
        /// </summary>
        /// <param name="other">List of element to add</param>
        public void Add(List<T> other)
        {
            foreach (T elem in other)
                Add(elem);
        }

        /// <summary>
        /// Add another <seealso cref="ValidationList&lt;T&gt;"/> to the current one
        /// </summary>
        /// <param name="other"><seealso cref="ValidationList&lt;T&gt;"/> to add</param>
        public void Add(ValidationList<T> other)
        {
            if (other.m_IsAllowed == m_IsAllowed)
                Add(other.m_List);
        }

        /// <summary>
        /// Check if the element is allowed by the list
        /// </summary>
        /// <remarks>
        /// The list will return true if:<br/>
        /// - The element is in the list and it's an allow list<br/>
        /// - The element is not in the list and it's a deny list
        /// </remarks>
        /// <param name="elem"></param>
        /// <returns>True if element is allowed by the list</returns>
        public bool Validate(T elem)
        {
            return m_List.Contains(elem) == m_IsAllowed;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        /// <summary>
        /// Implementation of <seealso cref="IEnumerator.MoveNext"/>
        /// </summary>
        /// <returns>true if enumerator is still in list</returns>
        public bool MoveNext()
        {
            m_EnumeratorPosition++;
            return (m_EnumeratorPosition < m_List.Count);
        }

        /// <summary>
        /// Implementation of <seealso cref="IEnumerator.Reset"/>
        /// </summary>
        public void Reset()
        {
            m_EnumeratorPosition = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                try
                {
#pragma warning disable CS8603 // Possible null reference return.
                    return m_List[m_EnumeratorPosition];
#pragma warning restore CS8603 // Possible null reference return.
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
