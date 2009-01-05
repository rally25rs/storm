using System;
using System.Reflection;
using System.Collections.Generic;
using Storm.DataBinders;

namespace Storm.Attributes
{
	/// <summary>
	/// Base interface for all attributes that indicate that a class is mapped.
	/// For instance, a class could be mapped to a Table (StormTableMapped), or
	///  to a stored procedure (StormProcedureMapped), etc... Each of these would
	///  implement this interface.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public abstract class ClassLevelMappedAttribute : Attribute
	{
		public StormPersistenceEvents SupressEvents { get; set; }
		public string DataBinder { get; set; }
		public Type AttachedTo { get; set; }
		public List<PropertyLevelMappedAttribute> PropertyAttributes { get; set; }
		protected bool Validated { get; set; }
		public bool DataBinderValidated { get; set; }

		protected ClassLevelMappedAttribute()
		{
			this.Validated = false;
			this.DataBinderValidated = false;
		}

		protected ClassLevelMappedAttribute GetClassLevelMappingAttribute(Type T)
		{
			object[] attributes = T.GetCustomAttributes(typeof(ClassLevelMappedAttribute), true);
			if (attributes == null || attributes.Length == 0)
				return null;
			return attributes[0] as ClassLevelMappedAttribute;
		}

		protected PropertyLevelMappedAttribute[] GetPropertyLevelMappingAttributes(PropertyInfo P)
		{
			object[] attributes = P.GetCustomAttributes(typeof(PropertyLevelMappedAttribute), true);
			if (attributes == null || attributes.Length == 0)
				return null;
			return attributes as PropertyLevelMappedAttribute[];
		}


		/// <summary>
		/// Validate an object's mapping before query execution.
		/// </summary>
		/// <param name="decoratedType">The Type that is mapped by this attribute.</param>
		/// <exception cref="StormConfigurationException">If mapping is invalid.</exception>
		internal virtual void ValidateMapping(Type decoratedType)
		{
			if (this.Validated)
				return;

			this.AttachedTo = decoratedType;

			// must have a data binder
			if (this.DataBinder == null || this.DataBinder.Length == 0)
				throw new StormConfigurationException("Invalid Mapping on Type [" + decoratedType.FullName + "]. DataBinder must be specified.");

			// validate all mapped properties
			var propArray = decoratedType.GetProperties();
			this.PropertyAttributes = new List<PropertyLevelMappedAttribute>(propArray.Length);
			try
			{
				foreach (PropertyInfo prop in propArray)
				{
					foreach (PropertyLevelMappedAttribute attrib in this.GetPropertyLevelMappingAttributes(prop))
					{
						attrib.ValidateMapping(prop);
						this.PropertyAttributes.Add(attrib);
					}
				}
				this.PropertyAttributes.TrimExcess();
			}
			catch
			{
				this.PropertyAttributes.Clear();
				this.PropertyAttributes = null;
				throw;
			}

			this.Validated = true;
		}
	}
}
