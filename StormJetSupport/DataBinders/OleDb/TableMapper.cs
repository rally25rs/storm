using System;
using System.Collections.Generic;
using System.Data.OleDb;
using Storm.Attributes;
using System.Text;

namespace Storm.DataBinders.OleDb
{
	internal sealed class TableMapper
	{
		public void PerformLoad<T>(T instanceToLoad, StormTableMappedAttribute mapping, OleDbConnection connection, DataCache dataCache)
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
			var cmd = connection.CreateCommand();
			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = cachedData.SelectQuery;
			try
			{
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormColumnMappedAttribute))
					{
						columnMapper.AddParameter(instanceToLoad, (StormColumnMappedAttribute)attrib, cmd);
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
				cmd.Dispose();
			}
		}

		private void BuildQueries(StormTableMappedAttribute mapping, DataCacheItem cachedData)
		{
			StringBuilder select = new StringBuilder("SELECT ");
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

			select.Append(columnsBuilder.ToString()).Append(" FROM ").Append(mapping.TableName).Append(" WHERE ").Append(keysBuilder.ToString());
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

			cachedData.SelectQuery = select.ToString();
			cachedData.InsertQuery = insert.ToString();
			cachedData.UpdateQuery = update.ToString();
			cachedData.DeleteQuery = delete.ToString();
			cachedData.ExistsQuery = exists.ToString();
		}
	}
}
