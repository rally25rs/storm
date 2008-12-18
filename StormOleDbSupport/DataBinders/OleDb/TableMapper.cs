using System;
using System.Collections.Generic;
using System.Data.OleDb;
using Storm.Attributes;
using System.Text;

namespace Storm.DataBinders.OleDb
{
	internal sealed class TableMapper
	{
		public void PerformLoad<T>(T instanceToLoad, StormTableMappedAttribute mapping, OleDbConnection connection, RecordLookupMode lookupMode, DataCache dataCache)
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
			if (lookupMode == RecordLookupMode.LookupByNonNullProperties)
				cmd = CommandBuilder.CreateSelectCommandForNonNull(mapping);
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
							if (lookupMode == RecordLookupMode.LookupByKeys && ((StormColumnMappedAttribute)attrib).PrimaryKey)
							{
								if (value == null)
									throw new StormPersistenceException("Property [" + attrib.AttachedTo.Name + "] is a primary key, but its value is null.");
								cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
							}
							else if (lookupMode == RecordLookupMode.LookupByNonNullProperties && value != null)
								cmd.Parameters[((StormColumnMappedAttribute)attrib).ColumnName].Value = value;
						}
					}
				}
				using (OleDbDataReader reader = cmd.ExecuteReader())
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

		private void BuildQueries(StormTableMappedAttribute mapping, DataCacheItem cachedData)
		{
			StringBuilder insert = new StringBuilder("INSERT INTO ");
			StringBuilder update = new StringBuilder("UPDATE ");
			StringBuilder delete = new StringBuilder("DELETE FROM ");
			StringBuilder exists = new StringBuilder("SELECT count(*) FROM ");

			// loop through mapped properties and add them into the queries
			List<string> keys = new List<string>();
			StringBuilder keysBuilder = new StringBuilder();
			StringBuilder columnsBuilder = new StringBuilder();
			StringBuilder columnsPairsBuilder = new StringBuilder();
			StringBuilder parametersBuilder = new StringBuilder();
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormColumnMappedAttribute))
				{
					StormColumnMappedAttribute column = (StormColumnMappedAttribute)attrib;
					if ((column.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
					{
						if (column.PrimaryKey)
						{
							keys.Add(column.ColumnName);
							if (keysBuilder.Length > 0)
								keysBuilder.Append(" AND ");
							keysBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);
						}
						else
						{
							if (columnsBuilder.Length > 0)
								columnsBuilder.Append(", ");
							columnsBuilder.Append(column.ColumnName);
							if (columnsPairsBuilder.Length > 0)
								columnsPairsBuilder.Append(", ");
							columnsPairsBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);
							if (parametersBuilder.Length > 0)
								parametersBuilder.Append(", ");
							parametersBuilder.Append(":").Append(column.ColumnName);
						}
					}
				}
			}

			update.Append(mapping.TableName).Append(" SET ").Append(columnsPairsBuilder.ToString()).Append(" WHERE ").Append(keysBuilder.ToString());
			delete.Append(mapping.TableName).Append(" WHERE ").Append(keysBuilder.ToString());
			exists.Append(mapping.TableName).Append(" WHERE ").Append(keysBuilder.ToString());
			insert.Append(mapping.TableName).Append(" (");
			for (int i = 0; i < keys.Count; i++)
			{
				if (i > 0)
					insert.Append(", ");
				insert.Append(keys[i]);
			}
			insert.Append(columnsBuilder.ToString()).Append(") VALUES (");
			for (int i = 0; i < keys.Count; i++)
			{
				if (i > 0)
					insert.Append(", ");
				insert.Append(":").Append(keys[i]);
			}
			if (keys.Count > 0)
				insert.Append(", ");
			insert.Append(parametersBuilder.ToString()).Append(")");

			cachedData.SelectCommand = CommandBuilder.CreateSelectCommandForKeys(mapping);
			cachedData.InsertQuery = insert.ToString();
			cachedData.UpdateQuery = update.ToString();
			cachedData.DeleteQuery = delete.ToString();
			cachedData.ExistsQuery = exists.ToString();
		}
	}
}
