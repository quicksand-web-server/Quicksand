namespace Quicksand
{
    public class IDGenerator
    {
        private int m_CurrentIdx;
        private List<int> m_FreeIdx = new();

        public IDGenerator(int idx = 0)
        {
            m_CurrentIdx = idx;
        }

        public int NextIdx()
        {
            int ret = m_CurrentIdx;
            if (m_FreeIdx.Count == 0)
                ++m_CurrentIdx;
            else
            {
                ret = m_FreeIdx[0];
                m_FreeIdx.RemoveAt(0);
            }
            return ret;
        }

        public void FreeIdx(int idx)
        {
            m_FreeIdx.Add(idx);
        }
    }
}
