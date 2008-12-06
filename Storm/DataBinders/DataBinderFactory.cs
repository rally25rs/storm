using System;
using System.Collections.Generic;

namespace Storm.DataBinders
{
	/// <summary>
	/// Used to get instances of the data binders.
	/// Currently treats each as a singleton, storing
	///  the instances for later use, instead of creating
	///  a new one for each persistance opertation.
	/// </summary>
	internal sealed class DataBinderFactory
	{
		/// <summary>
		/// As DataBinders are lazilly instantiated, they are stored here, keyed by their fully qualified type name.
		/// Note that the 'key' here matches the [StormTableMapped] attribute's DataBinder property.
		/// </summary>
		private static Dictionary<string, IDataBinder> LoadedDataBinders { get; set; }

		internal static IDataBinder GetDataBinder(string typeName)
		{
			if (LoadedDataBinders == null)
				LoadedDataBinders = new Dictionary<string, IDataBinder>();

			IDataBinder binder = null;
			lock (LoadedDataBinders)
			{
				binder = LoadedDataBinders[typeName];
				if (binder == null) // looks like we need to make one!
				{
					try
					{
						Type t = Type.GetType(typeName, false);
						if (t == null)
							throw new StormConfigurationException("DataBinderFacotry can not create an instance of [" + typeName + "]. Type or Assembly may be missing.");
						if (!t.GetInterface(typeof(IDataBinder).FullName))
							throw new StormConfigurationException("DataBinderFacotry can not create an instance of [" + typeName + "]. The Type does not implement IDataBinder.");
						binder = (IDataBinder)Activator.CreateInstance(t);
						LoadedDataBinders.Add(typeName, binder);
					}
					catch (Exception e)
					{
						throw new StormConfigurationException("DataBinderFacotry can not create an instance of [" + typeName + "].", e);
					}
				}
			}
			return binder;
		}

		/// <summary>
		/// How many DataBinder instances do we have created?
		/// Useful for stats/debugging.
		/// </summary>
		/// <returns></returns>
		internal static int CountLoadedDataBinders()
		{
			if (LoadedDataBinders == null)
				return 0;
			return LoadedDataBinders.Count;
		}

		/// <summary>
		/// Remove loaded Data Binders.
		/// </summary>
		internal static void DestroyLoadedDataBinders()
		{
			if (LoadedDataBinders != null)
				LoadedDataBinders.Clear();
		}
	}
}
