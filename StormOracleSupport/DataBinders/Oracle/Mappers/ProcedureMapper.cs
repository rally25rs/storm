using Oracle.DataAccess.Client;
using Storm.Attributes;

namespace Storm.DataBinders.Oracle.Mappers
{
	internal sealed class ProcedureMapper
	{
		public void PerformLoad<T>(T instanceToLoad, StormProcedureMappedAttribute mapping, OracleConnection connection, DataCache dataCache)
		{
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

			// build and execute command
			var cmd = cachedData.SelectCommand;
			try
			{
				cmd.Connection = connection;
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormParameterMappedAttribute))
					{
						StormParameterMappedAttribute paramAttrib = (StormParameterMappedAttribute)attrib;
						if ((paramAttrib.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
						{
							object value = paramAttrib.AttachedTo.GetValue(instanceToLoad, null);
							if (paramAttrib.ParameterDirection == System.Data.ParameterDirection.Input || paramAttrib.ParameterDirection == System.Data.ParameterDirection.InputOutput)
							{
								//if (value == null)
								//	throw new StormPersistenceException("Property [" + attrib.AttachedTo.Name + "] is an input parameter, but its value is null.");
								cmd.Parameters[paramAttrib.ParameterName].Value = value;
							}
						}
					}
				}
				int res = cmd.ExecuteNonQuery();
				// map results to object
				ParameterMapper parameterMapper = new ParameterMapper();
				foreach (var attrib in mapping.PropertyAttributes)
				{
					if (attrib.GetType() == typeof(StormParameterMappedAttribute))
					{
						parameterMapper.MapResult(instanceToLoad, (StormParameterMappedAttribute)attrib, cmd);
					}
				}
			}
			finally
			{
				cmd.ClearData();
			}
		}

		private void BuildQueries(StormProcedureMappedAttribute mapping, DataCacheItem cachedData)
		{
			cachedData.SelectCommand = CommandBuilder.CreateSelectCommandForProcedure(mapping);
		}

	}
}
