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
		#region IDataBinder Members

		public virtual void ValidateMapping(ClassLevelMappedAttribute mapping, IDbConnection connection)
		{
			return;
		}

		public abstract void Load<T>(T instanceToLoad, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);

		public abstract List<T> BatchLoad<T>(T instanceToLoad, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);

		public abstract void Persist<T>(T instanceToPersist, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);

		public abstract void BatchPersist<T>(List<T> listToPersist, IDbConnection connection, bool cascade);

		public abstract void Delete<T>(T instanceToDelete, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);

		#endregion
	}
}
