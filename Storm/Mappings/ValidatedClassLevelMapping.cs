using System;
using System.Collections.Generic;

namespace Storm.Mappings
{
	/// <summary>
	/// Performs and stores mapping validation for a class.
	/// For efficiency, we can save values here and cache them, avoiding
	///  further use of reflection and DB type checking.
	/// </summary>
	internal class ValidatedClassLevelMapping
	{
		internal bool PreValidated { get; set; }
		internal bool PostValidated { get; set; }

		protected ValidatedClassLevelMapping()
		{
			this.PreValidated = false;
			this.PostValidated = false;
		}

		/// <summary>
		/// Validate an object's mapping before executing a query.
		/// Typically check that the object is properly decorated, and
		///  that all key properties are populated.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The object whose mapping is being validated.</param>
		/// <returns></returns>
		internal virtual bool ValidateMappingPre<T>(T obj)
		{
			if (this.PreValidated)
				return true;

			if (this.HandledMappings == null)
				throw new StormConfigurationException("The Data Binder [" + this.GetType().FullName + "] does not have any handled mappings specified.");

			// get the mapping attributes for this object's Type.
			// First check the cache.
			ValidatedClassLevelMapping classMapping = ValidatedMappingCache.Get(typeof(T));
			if (classMapping == null) // if mapping not cached, we have to create it.
			{
				ClassLevelMappedAttribute attrib = GetMappingAttribute(obj.GetType());
				if (attrib == null)
				{
					this.PreValidated = false;
					return false;
				}

			}

			return this.PreValidated;
		}

		/// <summary>
		/// Validates as object's mapping after executing a query.
		/// Typically check that types properly map, and columns actually exist.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="obj">The object whose mapping is being validated.</param>
		/// <param name="schemaInfo">Some object that contains the data source's layout.</param>
		/// <returns></returns>
		internal virtual bool ValidateMappingPost<T, T2>(T obj, T2 schemaInfo)
		{
			if (this.PostValidated)
				return true;

			this.PostValidated = true;
		}
	}
}
