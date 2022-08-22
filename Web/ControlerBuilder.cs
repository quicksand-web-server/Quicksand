namespace Quicksand.Web
{
    /// <summary>
    /// Interface to build a controler
    /// </summary>
    public interface IControlerBuilder
    {
        /// <summary>
        /// Build a new controler
        /// </summary>
        /// <returns>The instance of the new controler, null if an error occured</returns>
        public Controler? Build();
    }

    /// <summary>
    /// Class to build a controler from delegate
    /// </summary>
    public class DelegateControlerBuilder : IControlerBuilder
    {
        /// <summary>
        /// Delegate to build a new controler
        /// </summary>
        /// <returns>The instance of the new controler, null if an error occured</returns>
        public delegate Controler? Delegate();

        private readonly Delegate m_Builder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builder">Delegate to build a new controler</param>
        public DelegateControlerBuilder(Delegate builder)
        {
            m_Builder = builder;
        }

        /// <summary>
        /// Build a new controler
        /// </summary>
        /// <returns>The instance of the new controler, null if an error occured</returns>
        public virtual Controler? Build() { return m_Builder(); }
    }

    /// <summary>
    /// Class to build a single instance of a controler
    /// </summary>
    public abstract class ASingletonControlerBuilder: IControlerBuilder
    {
        private Controler? m_Instance = default;

        /// <summary>
        /// Build a single instance of a controler
        /// </summary>
        /// <returns>The single instance of the controler, null if an error occured</returns>
        public virtual Controler? Build()
        {
            if (m_Instance == null)
                m_Instance = InternalBuild();
            return m_Instance;
        }

        /// <summary>
        /// Build a new controler
        /// </summary>
        /// <returns>The instance of the new controler, null if an error occured</returns>
        protected abstract Controler? InternalBuild();
    }

    /// <summary>
    /// Class to build a single instance of a controler
    /// </summary>
    public class DelegateSingletonControlerBuilder : DelegateControlerBuilder
    {
        private Controler? m_Instance = default;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builder">Delegate to build a new controler</param>
        public DelegateSingletonControlerBuilder(Delegate builder) : base(builder) {}

        /// <summary>
        /// Build a single instance of a controler
        /// </summary>
        /// <returns>The single instance of the controler, null if an error occured</returns>
        public sealed override Controler? Build()
        {
            if (m_Instance == null)
                m_Instance = base.Build();
            return m_Instance;
        }
    }
}
