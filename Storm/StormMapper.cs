using System;
using System.Collections.Generic;
using System.Reflection;
using Storm.DataBinders;
using Storm.Attributes;

namespace Storm
{
    /// <summary>
    /// This is the main class for Storm.
    /// StormMapper is used to load and persist objects.
    /// </summary>
	public class StormMapper
	{
		private static Dictionary<string, IDataBinder> DataBinders = new Dictionary<string, IDataBinder>();

		/// <summary>
		/// This class should not be instantiated. Use static methods instead.
		/// TODO: Would be better to make a singleton and/or a factory.
		/// </summary>
		internal StormMapper()
		{
		}

		/// <summary>
		/// Take an instance of a Storm mapped class and load the
		///  remaining data from the DB.
		/// </summary>
		/// <typeparam name="T">The type must be decorated with [StormTableMapped]</typeparam>
		/// <param name="instanceToLoad">An instance of the class to populate. All key properties must be populated.</param>
		/// <returns>Returns the loaded instance. Same as what was passed in.</returns>
		public static void Load<T>(T instanceToLoad)
		{
			Type instanceType = typeof(T);
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinders[attrib.DataBinder];
			if (binder == null)
				throw new StormConfigurationException("The Data Binder named [" + attrib.DataBinder + "] for Type [" + instanceType.FullName + "] is not registered.");
			attrib.ValidateMappingPre(instanceType);
			binder.Load(instanceToLoad, attrib);
		}

		/// <summary>
		/// Take an instance of a Storm mapped class and load the
		///   data into the DB. Performs insert or update as needed.
		/// </summary>
		/// <typeparam name="T">The type must be decorated with [StormTableMapped]</typeparam>
		/// <param name="instanceToPersist">An instance of the class to persist. All key properties must be populated.</param>
		public static void Persist<T>(T instanceToPersist)
		{
			Type instanceType = typeof(T);
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			if (!DataBinders.ContainsKey(attrib.DataBinder))
				throw new StormConfigurationException("The Data Binder named [" + attrib.DataBinder + "] for Type [" + instanceType.FullName + "] is not registered.");
			IDataBinder binder = DataBinders[attrib.DataBinder];
			attrib.ValidateMappingPre(instanceType);
			binder.Persist(instanceToPersist, attrib);
		}

		private static ClassLevelMappedAttribute GetMappingAttribute(Type T)
		{
			object[] attributes = T.GetCustomAttributes(typeof(ClassLevelMappedAttribute), true);
			if (attributes == null || attributes.Length == 0)
				return null;
			return attributes[0] as ClassLevelMappedAttribute;
		}

		/// <summary>
		/// Register a Data Binder instance to use.
		/// </summary>
		/// <param name="dataBinderName">The name of the DataBinder. Must be unique. Maps to the DataBinder property of class level mapping attributes.</param>
		/// <param name="dataBinder">The initialized Data Binder to associate with this name.</param>
		public static void RegisterDataBinder(string dataBinderName, IDataBinder dataBinder)
		{
			DataBinders.Add(dataBinderName, dataBinder);
		}

		/// <summary>
		/// Remove a previously registered Data Binder instance.
		/// </summary>
		/// <param name="dataBinderName">The name of the Data Binder to remove.</param>
		public static void RemoveDataBinder(string dataBinderName)
		{
			if(DataBinders.ContainsKey(dataBinderName))
				DataBinders.Remove(dataBinderName);
		}

		/// <summary>
		/// Perform shutdown / deinit actions.
		/// This should be called before program exit, like Dispose() would be.
		/// </summary>
		public static void Cleanup()
		{
			DataBinders.Clear();
		}
	}
}
