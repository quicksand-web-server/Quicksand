using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quicksand.Web.Html
{
    public class ValidationList<T>: IEnumerable, IEnumerator
    {
        private readonly bool m_IsAllowed;
        private readonly List<T> m_List = new();
        private int m_EnumeratorPosition = -1;

        public ValidationList(bool isAllowed)
        {
            m_IsAllowed = isAllowed;
        }

        public void Add(T elem)
        {
            m_List.Add(elem);
        }

        public bool Validate(T elem)
        {
            return m_List.Contains(elem) == m_IsAllowed;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this;
        }

        public bool MoveNext()
        {
            m_EnumeratorPosition++;
            return (m_EnumeratorPosition < m_List.Count);
        }

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
