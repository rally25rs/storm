using System;

namespace Storm
{
	/// <summary>
	/// Indicates that Storm encountered an error while persisting an object.
	/// </summary>
	public class StormPersistenceException : System.Exception
	{
        public StormPersistenceException() : base() { }
        public StormPersistenceException(string message) : base(message) { }
		public StormPersistenceException(string message, Exception innerException) : base(message, innerException) { }
	}
}
