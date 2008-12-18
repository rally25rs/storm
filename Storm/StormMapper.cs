using System;
using System.Collections.Generic;
using System.Reflection;
using Storm.DataBinders;
using Storm.Attributes;
using System.Data;

namespace Storm
{
    /// <summary>
    /// This is the main class for Storm.
    /// StormMapper is used to load and persist objects.
    /// </summary>
	public class StormMapper
	{
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
		/// <param name="connection">The Database Connection to run against.</param>
		/// <returns>Returns the loaded instance. Same as what was passed in.</returns>
		public static void Load<T>(T instanceToLoad, IDbConnection connection)
		{
			Load(instanceToLoad, connection, true);
		}

		/// <summary>
		/// Take an instance of a Storm mapped class and load the
		///  remaining data from the DB.
		/// </summary>
		/// <typeparam name="T">The type must be decorated with [StormTableMapped]</typeparam>
		/// <param name="instanceToLoad">An instance of the class to populate. All key properties must be populated.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the load (recursive load). If true, any properties that are mapped to other mapped objects will also be loaded.</param>
		/// <returns>Returns the loaded instance. Same as what was passed in.</returns>
		public static void Load<T>(T instanceToLoad, IDbConnection connection, bool cascade)
		{
			Type instanceType = typeof(T);
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinderFactory.GetDataBinder(connection.ConnectionString, attrib.DataBinder);
			attrib.ValidateMapping(instanceType);
			binder.ValidateMapping(attrib, connection);
			binder.Load(instanceToLoad, attrib, RecordLookupMode.LookupByKeys, connection, cascade);
		}


		/// <summary>
		/// Take an instance of a Storm mapped class and load the
		///   data into the DB. Performs insert or update as needed.
		/// </summary>
		/// <typeparam name="T">The type must be decorated with [StormTableMapped]</typeparam>
		/// <param name="instanceToPersist">An instance of the class to persist. All key properties must be populated.</param>
		public static void Persist<T>(T instanceToPersist, IDbConnection connection, bool cascade)
		{
			Type instanceType = typeof(T);
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinderFactory.GetDataBinder(connection.ConnectionString, attrib.DataBinder);
			attrib.ValidateMapping(instanceType);
			binder.ValidateMapping(attrib, connection);
			binder.Persist(instanceToPersist, attrib, connection);
		}

		private static ClassLevelMappedAttribute GetMappingAttribute(Type T)
		{
			object[] attributes = T.GetCustomAttributes(typeof(ClassLevelMappedAttribute), true);
			if (attributes == null || attributes.Length == 0)
				return null;
			return attributes[0] as ClassLevelMappedAttribute;
		}

		/// <summary>
		/// Perform shutdown / deinit actions.
		/// This should be called before program exit, like Dispose() would be.
		/// </summary>
		public static void Cleanup()
		{
			DataBinderFactory.ClearDataBinders();
		}
	}
}
