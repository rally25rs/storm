using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using Storm.Attributes;

namespace Storm.DataBinders.Oracle.Mappers
{
	internal sealed class TableMapper
	{
		public void PerformLoad<T>(T instanceToLoad, StormTableMappedAttribute mapping, OracleConnection connection, DataCache dataCache)
		{
			ColumnMapper columnMapper = new ColumnMapper();

			// make sure this action isn't supressed
			if ((mapping.SupressEvents & StormPersistenceEvents.Load) == StormPersistenceEvents.Load)
				throw new StormPersistenceException("Unable to load [" + instanceToLoad.GetType().FullName + "]. Load event is supressed.");

			// check the data cache for info about this mapping
			DataCacheItem cachedData = dataCache.Get(instanceToLoad.GetType());
			if (cachedData == null)
			{
				cachedData = new DataCacheItem();
				this.BuildQueries(mapping, cachedData);
				dataCache.Add(instanceToLoad.GetType(), cachedData);
			}

			// build and execute query
			var cmd = cachedData.SelectCommand;
			try
			{
				cmd.Connection = connection;
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormColumnMappedAttribute))
					{
						if ((attrib.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
						{
							object value = attrib.AttachedTo.GetValue(instanceToLoad, null);
							if (((StormColumnMappedAttribute)attrib).PrimaryKey)
							{
								if (value == null)
									throw new StormPersistenceException("Property [" + attrib.AttachedTo.Name + "] is a primary key, but its value is null.");
								cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
							}
						}
					}
				}
				using (OracleDataReader reader = cmd.ExecuteReader())
				{
					// map results to object
					if (!reader.Read())
						throw new StormPersistenceException("Unable to load instance of [" + instanceToLoad.GetType().FullName + "]. No data returned from DB.");
					foreach (var attrib in mapping.PropertyAttributes)
					{
						if (attrib.GetType() == typeof(StormColumnMappedAttribute))
						{
							if(((StormColumnMappedAttribute)attrib).PrimaryKey == false)
								columnMapper.MapResult(instanceToLoad, (StormColumnMappedAttribute)attrib, reader);
						}
					}
					reader.Close();
				}
			}
			finally
			{
				cmd.ClearData();
			}
		}

		public List<T> PerformBatchLoad<T>(T instanceToLoad, StormTableMappedAttribute mapping, OracleConnection connection, DataCache dataCache)
		{
			ColumnMapper columnMapper = new ColumnMapper();
			List<T> loadedObjects = new List<T>();

			// make sure this action isn't supressed
			if ((mapping.SupressEvents & StormPersistenceEvents.Load) == StormPersistenceEvents.Load)
				throw new StormPersistenceException("Unable to load [" + instanceToLoad.GetType().FullName + "]. Load event is supressed.");

			// build and execute query
			var cmd = CommandBuilder.CreateSelectCommandForNonNull(instanceToLoad, mapping);
			try
			{
				cmd.Connection = connection;
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormColumnMappedAttribute))
					{
						if ((attrib.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
						{
							object value = attrib.AttachedTo.GetValue(instanceToLoad, null);
							if (value != null)
								cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
						}
					}
				}
				using (OracleDataReader reader = cmd.ExecuteReader())
				{
					// map results to objects
					if (!reader.HasRows)
						throw new StormPersistenceException("Unable to load instance of [" + instanceToLoad.GetType().FullName + "]. No data returned from DB.");
					while (reader.Read())
					{
						T objToLoad = Activator.CreateInstance<T>();
						foreach (var attrib in mapping.PropertyAttributes)
						{
							if (attrib.GetType() == typeof(StormColumnMappedAttribute))
							{
								columnMapper.MapResult(objToLoad, (StormColumnMappedAttribute)attrib, reader);
							}
						}
						loadedObjects.Add(objToLoad);
					}
					reader.Close();
				}
			}
			finally
			{
				cmd.ClearData();
			}
			loadedObjects.TrimExcess();
			return loadedObjects;
		}

		public void PerformPersist<T>(T instanceToLoad, StormTableMappedAttribute mapping, OracleConnection connection, DataCache dataCache)
		{
			// check the data cache for info about this mapping
			DataCacheItem cachedData = dataCache.Get(instanceToLoad.GetType());
			if (cachedData == null)
			{
				cachedData = new DataCacheItem();
				this.BuildQueries(mapping, cachedData);
				dataCache.Add(instanceToLoad.GetType(), cachedData);
			}

			// see if this data already exists, so we know if we are going to insert or update.
			if (this.PerformExists(instanceToLoad, mapping, connection, cachedData.ExistsCommand))
				this.PerformUpdate(instanceToLoad, mapping, connection, cachedData.UpdateCommand);
			else
				this.PerformInsert(instanceToLoad, mapping, connection, cachedData.InsertCommand);

		}

		public void PerformDelete<T>(T instanceToDelete, StormTableMappedAttribute mapping, OracleConnection connection, DataCache dataCache)
		{
			// make sure this action isn't supressed
			if ((mapping.SupressEvents & StormPersistenceEvents.Delete) == StormPersistenceEvents.Delete)
				throw new StormPersistenceException("Unable to delete [" + instanceToDelete.GetType().FullName + "]. Delete event is supressed.");

			// check the data cache for info about this mapping
			DataCacheItem cachedData = dataCache.Get(instanceToDelete.GetType());
			if (cachedData == null)
			{
				cachedData = new DataCacheItem();
				this.BuildQueries(mapping, cachedData);
				dataCache.Add(instanceToDelete.GetType(), cachedData);
			}

			// build and execute query
			var cmd = cachedData.DeleteCommand;
			try
			{
				cmd.Connection = connection;
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormColumnMappedAttribute))
					{
						if ((attrib.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
						{
							object value = attrib.AttachedTo.GetValue(instanceToDelete, null);
							if (((StormColumnMappedAttribute)attrib).PrimaryKey)
							{
								if (value == null)
									throw new StormPersistenceException("Property [" + attrib.AttachedTo.Name + "] is a primary key, but its value is null.");
								cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
							}
						}
					}
				}
				cmd.ExecuteScalar();
			}
			finally
			{
				cmd.ClearData();
			}
		}

		private void PerformInsert(object instanceToLoad, StormTableMappedAttribute mapping, OracleConnection connection, OracleCommand cmd)
		{
			// make sure this action isn't supressed
			if ((mapping.SupressEvents & StormPersistenceEvents.Insert) == StormPersistenceEvents.Insert)
				throw new StormPersistenceException("Unable to insert [" + instanceToLoad.GetType().FullName + "]. Insert event is supressed.");

			// build and execute query
			try
			{
				cmd.Connection = connection;
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormColumnMappedAttribute))
					{
						if ((attrib.SupressEvents & StormPersistenceEvents.Insert) != StormPersistenceEvents.Insert)
						{
							object value = attrib.AttachedTo.GetValue(instanceToLoad, null) ?? System.DBNull.Value;
							if (((StormColumnMappedAttribute)attrib).PrimaryKey && value == System.DBNull.Value)
							{
								throw new StormPersistenceException("Property [" + attrib.AttachedTo.Name + "] is a primary key, but its value is null.");
							}
							cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
						}
					}
				}
				object result = cmd.ExecuteScalar();
			}
			finally
			{
				cmd.ClearData();
			}
		}

		private void PerformUpdate(object instanceToLoad, StormTableMappedAttribute mapping, OracleConnection connection, OracleCommand cmd)
		{
			// make sure this action isn't supressed
			if ((mapping.SupressEvents & StormPersistenceEvents.Update) == StormPersistenceEvents.Update)
				throw new StormPersistenceException("Unable to update [" + instanceToLoad.GetType().FullName + "]. Update event is supressed.");

			// build and execute query
			try
			{
				cmd.Connection = connection;
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormColumnMappedAttribute))
					{
						if ((attrib.SupressEvents & StormPersistenceEvents.Update) != StormPersistenceEvents.Update)
						{
							object value = attrib.AttachedTo.GetValue(instanceToLoad, null) ?? System.DBNull.Value;
							if (((StormColumnMappedAttribute)attrib).PrimaryKey && value == System.DBNull.Value)
							{
								throw new StormPersistenceException("Property [" + attrib.AttachedTo.Name + "] is a primary key, but its value is null.");
							}
							cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
						}
					}
				}
				object result = cmd.ExecuteScalar();
			}
			finally
			{
				cmd.ClearData();
			}
		}

		private bool PerformExists<T>(T instanceToLoad, StormTableMappedAttribute mapping, OracleConnection connection, OracleCommand cmd)
		{
			bool exists = false;
			try
			{
				cmd.Connection = connection;
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormColumnMappedAttribute))
					{
						if ((attrib.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
						{
							object value = attrib.AttachedTo.GetValue(instanceToLoad, null);
							if (((StormColumnMappedAttribute)attrib).PrimaryKey)
							{
								if (value == null)
									throw new StormPersistenceException("Property [" + attrib.AttachedTo.Name + "] is a primary key, but its value is null.");
								cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
							}
						}
					}
				}
				object result = cmd.ExecuteScalar();
				if (((result as int?) ?? 0) > 0)
					exists = true;
			}
			finally
			{
				cmd.ClearData();
			}
			return exists;
		}

		private void BuildQueries(StormTableMappedAttribute mapping, DataCacheItem cachedData)
		{
			cachedData.SelectCommand = CommandBuilder.CreateSelectCommandForKeys(mapping);
			cachedData.InsertCommand = CommandBuilder.CreateInsertCommand(mapping);
			cachedData.UpdateCommand = CommandBuilder.CreateUpdateCommand(mapping);
			cachedData.DeleteCommand = CommandBuilder.CreateDeleteCommand(mapping);
			cachedData.ExistsCommand = CommandBuilder.CreateExistsCommand(mapping);
		}
	}
}
