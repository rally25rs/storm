using System;
using System.Reflection;
using System.Collections.Generic;

namespace Storm.DataBinders
{
	/// <summary>
	/// Creates instances of Data Binders.
	/// Each data binder should be trated as a singleton per connection
	/// for caching purposes. This factory handles that relationship.
	/// </summary>
	internal sealed class DataBinderFactory
	{
		private static Dictionary<string, Dictionary<string, IDataBinder>> dataBinders = new Dictionary<string, Dictionary<string, IDataBinder>>();

		/// <summary>
		/// Get a data binder instance associated with the gives connection string.
		/// </summary>
		/// <param name="dbConnectionString">The connection string that the data binder is associated with.</param>
		/// <param name="dataBinderName">The full type name of the data binder.</param>
		/// <returns>An instance of the Data Binder.</returns>
		internal static IDataBinder GetDataBinder(string dbConnectionString, string dataBinderName)
		{
			// first check for an existing data binder instance.
			if(dataBinders.ContainsKey(dbConnectionString))
			{
				var bindersByName = dataBinders[dbConnectionString];
				if (bindersByName.ContainsKey(dataBinderName))
				{
					return bindersByName[dataBinderName];
				}
			}

			// if no existing instance found, then we need to create one.
			return CreateDataBinder(dbConnectionString, dataBinderName);
		}

		/// <summary>
		/// Removes cached data binders associated with the given connection string.
		/// </summary>
		/// <param name="dbConnectionString"></param>
		internal static void ClearDataBinders(string dbConnectionString)
		{
			if(dataBinders.ContainsKey(dbConnectionString))
			{
				dataBinders[dbConnectionString].Clear();
				dataBinders.Remove(dbConnectionString);
			}
		}

		/// <summary>
		/// Removes all cached data binders.
		/// </summary>
		internal static void ClearDataBinders()
		{
			var binderEnum = dataBinders.GetEnumerator();
			while (binderEnum.MoveNext())
				binderEnum.Current.Value.Clear();
			dataBinders.Clear();
		}

		private static IDataBinder CreateDataBinder(string dbConnectionString, string dataBinderName)
		{
			lock (dataBinders)
			{
				if (dataBinders.ContainsKey(dbConnectionString) && dataBinders[dbConnectionString].ContainsKey(dataBinderName))
					return dataBinders[dbConnectionString][dataBinderName];
				try
				{
					IDataBinder newBinder = (IDataBinder)(Activator.CreateInstance(Type.GetType(dataBinderName)));
					if (!dataBinders.ContainsKey(dbConnectionString))
						dataBinders.Add(dbConnectionString, new Dictionary<string, IDataBinder>());
					Dictionary<string, IDataBinder> bindersByName = dataBinders[dbConnectionString];
					bindersByName.Add(dataBinderName, newBinder);
					return newBinder;
				}
				catch (Exception e)
				{
					throw new StormConfigurationException("Unable to create an instance of Data Binder with type [" + dataBinderName + "].", e);
				}
			}
		}
	}
}
