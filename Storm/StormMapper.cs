using System;
using System.Collections.Generic;
using System.Reflection;
using Storm.DataBinders;
using Storm.Attributes;
using System.Data;
using System.Reflection.Emit;

namespace Storm
{
    /// <summary>
    /// This is the main class for Storm.
    /// StormMapper is used to load and persist objects.
    /// </summary>
	public class StormMapper
	{
		private static MethodInfo loadMethodInfo = null;
		private static MethodInfo batchLoadMethodInfo = null;

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
		/// <typeparam name="T">A Storm mapped type.</typeparam>
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
		/// <typeparam name="T">A Storm mapped type.</typeparam>
		/// <param name="instanceToLoad">An instance of the class to populate. All key properties must be populated.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the load (recursive load). If true, any properties that are mapped to other mapped objects will also be loaded.</param>
		/// <returns>Returns the loaded instance. Same as what was passed in.</returns>
		public static void Load<T>(T instanceToLoad, IDbConnection connection, bool cascade)
		{
			Type instanceType = instanceToLoad.GetType();
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinderFactory.GetDataBinder(connection.ConnectionString, attrib.DataBinder);
			binder.ValidateMapping(attrib, connection);
			binder.Load(instanceToLoad, attrib, connection, cascade);

			if (cascade)
				CascadeLoad(instanceToLoad, connection);
		}

		/// <summary>
		/// Take an instance of a Storm mapped class and load a list of matching objects
		/// from the DB. Uses all non-null mapped properties in the object to query on.
		/// </summary>
		/// <typeparam name="T">A Storm mapped type.</typeparam>
		/// <param name="instanceToLoad">An instance of the class to populate.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <returns>Returns a List of loaded instances.</returns>
		public static List<T> BatchLoad<T>(T instanceToLoad, IDbConnection connection)
		{
			return BatchLoad(instanceToLoad, connection, true);
		}

		/// <summary>
		/// Take a Type of a Storm mapped class and load a list of matching objects
		/// from the DB. Does not query on any specific values. Useful for loading whole tables.
		/// </summary>
		/// <typeparam name="T">A Storm mapped type.</typeparam>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <returns>Returns a List of loaded instances.</returns>
		public static List<T> BatchLoad<T>(IDbConnection connection)
		{
			return BatchLoad<T>(connection, true);
		}

		/// <summary>
		/// Take a Type of a Storm mapped class and load a list of matching objects
		/// from the DB. Does not query on any specific values. Useful for loading whole tables.
		/// </summary>
		/// <typeparam name="T">A Storm mapped type.</typeparam>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the load (recursive load). If true, any properties that are mapped to other mapped objects will also be loaded.</param>
		/// <returns>Returns a List of loaded instances.</returns>
		public static List<T> BatchLoad<T>(IDbConnection connection, bool cascade)
		{
			return BatchLoad(Activator.CreateInstance<T>(), connection, cascade);
		}

		/// <summary>
		/// Take an instance of a Storm mapped class and load a list of matching objects
		/// from the DB. Uses all non-null mapped properties in the object to query on.
		/// </summary>
		/// <typeparam name="T">A Storm mapped type.</typeparam>
		/// <param name="instanceToLoad">An instance of the class to populate.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the load (recursive load). If true, any properties that are mapped to other mapped objects will also be loaded.</param>
		/// <returns>Returns a List of loaded instances.</returns>
		public static List<T> BatchLoad<T>(T instanceToLoad, IDbConnection connection, bool cascade)
		{
			Type instanceType = instanceToLoad.GetType();
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinderFactory.GetDataBinder(connection.ConnectionString, attrib.DataBinder);
			binder.ValidateMapping(attrib, connection);
			List<T> retList = binder.BatchLoad(instanceToLoad, attrib, connection, cascade);

			if (cascade)
			{
				foreach (T instance in retList)
					CascadeLoad(instance, connection);
			}

			return retList;
		}

		public static void Persist<T>(T instanceToPersist, IDbConnection connection)
		{
			Persist(instanceToPersist, connection, true);
		}

		/// <summary>
		/// Take an instance of a Storm mapped class and load the
		///   data into the DB. Performs insert or update as needed.
		/// </summary>
		/// <typeparam name="T">A Storm mapped type.</typeparam>
		/// <param name="instanceToPersist">An instance of the class to persist. All key properties must be populated.</param>
		public static void Persist<T>(T instanceToPersist, IDbConnection connection, bool cascade)
		{
			Type instanceType = instanceToPersist.GetType();
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinderFactory.GetDataBinder(connection.ConnectionString, attrib.DataBinder);
			binder.ValidateMapping(attrib, connection);
			binder.Persist(instanceToPersist, attrib, connection, cascade);
		}

		public static void Delete<T>(T instanceToPersist, IDbConnection connection)
		{
			Delete(instanceToPersist, connection, false);
		}

		public static void Delete<T>(T instanceToPersist, IDbConnection connection, bool cascade)
		{
			Type instanceType = instanceToPersist.GetType();
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormPersistenceException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinderFactory.GetDataBinder(connection.ConnectionString, attrib.DataBinder);
			binder.ValidateMapping(attrib, connection);
			binder.Delete (instanceToPersist, attrib, connection, cascade);
		}

		private static ClassLevelMappedAttribute GetMappingAttribute(Type T)
		{
			ClassLevelMappedAttribute[] attributes = T.GetCachedAttributes<ClassLevelMappedAttribute>(true);
			if (attributes == null || attributes.Length == 0)
				return null;
			attributes[0].ValidateMapping(T);
			return attributes[0];
		}

		public static void ValidateMapping<T>(T instanceToPersist, IDbConnection connection)
		{
			Type instanceType = typeof(T);
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormConfigurationException("The Type [" + instanceType.FullName + "] is not mapped.");
			IDataBinder binder = DataBinderFactory.GetDataBinder(connection.ConnectionString, attrib.DataBinder);
			binder.ValidateMapping(attrib, connection);
		}

		/// <summary>
		/// Perform shutdown / deinit actions.
		/// This should be called before program exit, like Dispose() would be.
		/// </summary>
		public static void Cleanup()
		{
			DataBinderFactory.ClearDataBinders();
		}

		private static void CascadeLoad<T>(T instanceToCascadeFrom, IDbConnection connection)
		{
			Type instanceType = instanceToCascadeFrom.GetType();
			ClassLevelMappedAttribute attrib = GetMappingAttribute(instanceType);
			if (attrib == null)
				throw new StormConfigurationException("The Type [" + instanceType.FullName + "] is not mapped.");
			if (!attrib.SupportsCascade)
				return;

			PropertyInfo[] properties = instanceType.GetProperties();
			foreach (PropertyInfo propInf in properties)
			{
				// get StormRelationMappedAttributes for this property
				StormRelationMappedAttribute[] relations = propInf.GetCachedAttributes<StormRelationMappedAttribute>(true);
				if (relations != null && relations.Length > 0)
				{
					// make a new instance of the desired type and set its initial values.
					Type typeToCreate = propInf.PropertyType;
					Type[] generics = propInf.PropertyType.GetGenericArguments();
					if (generics != null && generics.Length == 1 && propInf.PropertyType.FullName.Contains("System.Collections.Generic.List"))
						typeToCreate = generics[0];
					object relatedInstance = Activator.CreateInstance(typeToCreate);
					foreach (StormRelationMappedAttribute relation in relations)
					{
						relation.ValidateMapping(propInf);
						relation.RelatedTo.SetValue(relatedInstance, relation.RelatedFrom.GetValue(instanceToCascadeFrom, null), null);
					}

					// if the related property is a collection, we need to BatchLoad.
					if (generics != null && generics.Length == 1 && propInf.PropertyType.FullName.Contains("System.Collections.Generic.List"))
					{
						object loaded = GetBatchLoadMethodInfo().MakeGenericMethod(relatedInstance.GetType()).Invoke(null, new object[] { relatedInstance, connection, true });
						propInf.SetValue(instanceToCascadeFrom, loaded, null);
					}
					else
					{
						propInf.SetValue(instanceToCascadeFrom, relatedInstance, null);
						GetLoadMethodInfo().MakeGenericMethod(relatedInstance.GetType()).Invoke(null, new object[] { relatedInstance, connection, true });
					}
				}
			}
		}

		private static MethodInfo GetLoadMethodInfo()
		{
			if (loadMethodInfo == null)
			{

				foreach (MethodInfo mi in typeof(StormMapper).GetMethods(BindingFlags.Static | BindingFlags.Public))
				{
					if (mi.Name == "Load" && mi.GetParameters().Length == 3 && mi.GetGenericArguments().Length == 1)
					{
						loadMethodInfo = mi;
						break;
					}
				}
			}
			return loadMethodInfo;
		}

		private static MethodInfo GetBatchLoadMethodInfo()
		{
			if (batchLoadMethodInfo == null)
			{

				foreach (MethodInfo mi in typeof(StormMapper).GetMethods(BindingFlags.Static | BindingFlags.Public))
				{
					if (mi.Name == "BatchLoad" && mi.GetParameters().Length == 3 && mi.GetGenericArguments().Length == 1)
					{
						batchLoadMethodInfo = mi;
						break;
					}
				}
			}
			return batchLoadMethodInfo;
		}

	}
}
