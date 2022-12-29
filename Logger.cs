namespace Quicksand
{
    /// <summary>
    /// Class used to log within quicksand
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Type of log
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// Websocket logs
            /// </summary>
            WEBSOCKET,
            /// <summary>
            /// HTTP logs
            /// </summary>
            HTTP,
            /// <summary>
            /// TCP logs
            /// </summary>
            TCP
        }

        /// <summary>
        /// Delegate used by the logger
        /// </summary>
        /// <param name="type">Type of message logged</param>
        /// <param name="message">Message logged</param>
        public delegate void LoggerDelegate(LogType type, string message);

        private static List<LoggerDelegate> ms_GlobalLogger = new();

        /// <summary>
        /// Register a logger
        /// </summary>
        /// <param name="logger">Logger delegate</param>
        public static void RegisterLogger(LoggerDelegate logger) => ms_GlobalLogger.Add(logger);

        /// <summary>
        /// Unregister a logger
        /// </summary>
        /// <param name="logger">Logger delegate</param>
        public static void UnregisterLogger(LoggerDelegate logger) => ms_GlobalLogger.Remove(logger);

        internal static void Log(LogType type, string message)
        {
            foreach (var logger in ms_GlobalLogger)
                logger(type, message);
        }
    }
}
