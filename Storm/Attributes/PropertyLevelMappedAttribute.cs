using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Storm.Attributes
{
	/// <summary>
	/// Base interface for all attributes that indicate that a property is mapped.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public abstract class PropertyLevelMappedAttribute : System.Attribute
	{
		public PropertyInfo AttachedTo { get; set; }

		public StormPersistenceEvents SupressEvents { get; set; }

		protected bool Validated { get; set; }

		protected PropertyLevelMappedAttribute() : base()
		{
		}

		/// <summary>
		/// Validate a peroperty's mapping before query execution.
		/// </summary>
		/// <param name="decoratedProperty">The Property that is mapped by this attribute.</param>
		/// <exception cref="StormConfigurationException">If mapping is invalid.</exception>
		internal virtual void ValidateMapping(PropertyInfo decoratedProperty)
		{
			if (this.Validated)
				return;

			this.AttachedTo = decoratedProperty;
			this.Validated = true;
		}

	}
}
