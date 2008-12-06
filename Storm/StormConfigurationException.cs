using System;

namespace Storm
{
    /// <summary>
    /// Indicates a configuration problem with Storm.
    /// </summary>
    class StormConfigurationException : System.Exception
    {
        public StormConfigurationException() : base() { }
        public StormConfigurationException(string message) : base(message) { }
        public StormConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
