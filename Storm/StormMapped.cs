using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Storm.Attributes;

namespace Storm
{
	/// <summary>
	/// Classes mapped by Storm can optionally extend this class to provide convenience methods.
	/// In addition to implementing IStormMapped (primarily by calling static methods on StormMapper),
	///  this abstract class also provides a facility for determining if the object has changed, and
	///  needs to be updated. This means that using .Persist() can be more efficient, since the
	///  query will only be run if the object actually changed. If StormMapped is not extended,
	///  calling StormMapper.Persist() will always run a query to update the data store, even
	///  if there are no actual changes.
	/// However, that also means more memory is used, since a copy of the original needs to be
	///  maintained for comparison purposes (I think Hibernate does the same thing; mutable
	///  object are duplicated in memory).
	/// </summary>
	public abstract class StormMapped : IStormMapped
	{
		// will hold the values of mutable properties for later comparison. keyed on property name.
		private Dictionary<string, object> _stormTrackedPropertyValues = null;

		public StormMapped()
		{
		}

		#region IStormMapped Members

		/// <summary>
		/// Whether or not any properties on this object have been changed that need to be persisted.
		/// This property will automatically be checked by the .Persist() methods, so you do not need
		///  to check this prior to calling .Persist().
		/// </summary>
		public bool HasChangesToPersist
		{
			get
			{
				if (this._stormTrackedPropertyValues == null)
					return false;

				try
				{
					Dictionary<string, PropertyInfo> properties = this.GetType().GetProperties().ToDictionary(v => v.Name, v => v);
					foreach (var entry in _stormTrackedPropertyValues)
					{
						if (properties[entry.Key].GetValue(this, null) != entry.Value)
							return true;
					}
					return false;
				}
				catch(Exception e)
				{
					throw new StormPersistenceException("Error determining if the object of type [" + this.GetType().FullName + "] has changes to persist.", e);
				}
			}
		}

		public void Load(IDbConnection connection)
		{
			StormMapper.Load(this, connection);
		}

		public void Load(IDbConnection connection, bool cascade)
		{
			StormMapper.Load(this, connection, cascade);
		}

		public void Persist(IDbConnection connection)
		{
			StormMapper.Persist(this, connection);
		}

		public void Persist(IDbConnection connection, bool cascade)
		{
			StormMapper.Persist(this, connection, cascade);
		}

		public void Delete(IDbConnection connection)
		{
			StormMapper.Delete(this, connection);
		}

		public void Delete(IDbConnection connection, bool cascade)
		{
			StormMapper.Delete(this, connection, cascade);
		}

		/// <summary>
		/// scan all properties for PropertyMappingAttributes that are insert/updateable. save them to _stormTrackedPropertyValues.
		/// </summary>
		public void StormLoaded()
		{
			ClassLevelMappedAttribute[] classAttribArr = this.GetType().GetCachedAttributes<ClassLevelMappedAttribute>(true);
			foreach (ClassLevelMappedAttribute classAttrib in classAttribArr)
			{
				if ((classAttrib.SupressEvents & (StormPersistenceEvents.Insert | StormPersistenceEvents.Update)) != (StormPersistenceEvents.Insert | StormPersistenceEvents.Update))
				{
					foreach (PropertyLevelMappedAttribute propAttrib in classAttrib.PropertyAttributes)
					{
						if (!(propAttrib is StormRelationMappedAttribute) && (classAttrib.SupressEvents & (StormPersistenceEvents.Insert | StormPersistenceEvents.Update)) != (StormPersistenceEvents.Insert | StormPersistenceEvents.Update))
						{
							if (this._stormTrackedPropertyValues == null)	// don't create until we know there is something to store.
								this._stormTrackedPropertyValues = new Dictionary<string, object>();

							this._stormTrackedPropertyValues.Add(propAttrib.AttachedTo.Name, propAttrib.AttachedTo.GetValue(this, null));
						}
					}
				}
			}
		}

		#endregion
	}
}
