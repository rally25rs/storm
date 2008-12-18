using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Storm.Attributes;
using System.Data;

namespace Storm.DataBinders
{
	/// <summary>
	/// An abstract implementation of IDataBinder.
	/// Provides some utility and common methods for other DataBinder
	/// implementations to use.
	/// </summary>
	public abstract class AbstractDataBinder : IDataBinder
	{
		/// <summary>
		/// A list of class level mapping attributes that this Data Binder knows how to handle.
		/// For example a particular Data Binder may know how to map objects to tables, but not to stored procedures.
		/// </summary>
		protected virtual ClassLevelMappedAttribute[] HandledMappings { get; set; }

		#region IDataBinder Members

		public virtual void ValidateMapping(ClassLevelMappedAttribute mapping, IDbConnection connection)
		{
			return;
		}

		public abstract void Load<T>(T instanceToLoad, ClassLevelMappedAttribute mapping, RecordLookupMode lookupMode, IDbConnection connection, bool cascade);

		public abstract void Persist<T>(T instanceToPersist, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);

		public abstract void BatchPersist<T>(List<T> listToPersist, IDbConnection connection, bool cascade);

		public abstract void Delete<T>(T instanceToDelete, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);

		#endregion
	}
}
