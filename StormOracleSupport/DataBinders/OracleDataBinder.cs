using System;
using System.Collections.Generic;
using System.Data;
using Oracle.DataAccess.Client;
using Storm.Attributes;
using Storm.DataBinders.Oracle;
using Storm.DataBinders.Oracle.Validation;
using Storm.DataBinders.Oracle.Mappers;

namespace Storm.DataBinders
{
	/// <summary>
	/// Data Binder implementation for the Oracle ADO.NET driver.
	/// Note that this is for the Oracle data provider, NOT the Microsoft Oracle data provider.
	/// </summary>
	class OracleDataBinder : AbstractDataBinder
	{
		private DataCache dataCache = null;

		public OracleDataBinder() : base()
		{
			this.dataCache = new DataCache();
		}

		public override void ValidateMapping(ClassLevelMappedAttribute mapping, IDbConnection connection)
		{
			if (mapping.DataBinderValidated)
				return;

			try
			{
				OracleConnection oraConnection = this.VerifyConnection(connection);
				SchemaValidator validator = new SchemaValidator(oraConnection);
				validator.VerifyMapping(mapping);
				mapping.DataBinderValidated = true;
			}
			catch (Exception e)
			{
				throw new StormConfigurationException("OracleDataBinder found an invalid mapping for type [" + mapping.AttachedTo.FullName + "].", e);
			}
		}

		public override void Load<T>(T instanceToLoad, Storm.Attributes.ClassLevelMappedAttribute mapping, System.Data.IDbConnection connection, bool cascade)
		{
			try
			{
				OracleConnection oraConnection = this.VerifyConnection(connection);
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					mapper.PerformLoad(instanceToLoad, (StormTableMappedAttribute)mapping, oraConnection, dataCache);
				}
				else if (mappingType == typeof(StormProcedureMappedAttribute))
				{
					var mapper = new ProcedureMapper();
					mapper.PerformLoad(instanceToLoad, (StormProcedureMappedAttribute)mapping, oraConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OracleDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Load instance of type [" + instanceToLoad.GetType().FullName + "].", e);
			}
		}

		public override List<T> BatchLoad<T>(T instanceToLoad, Storm.Attributes.ClassLevelMappedAttribute mapping, System.Data.IDbConnection connection, bool cascade)
		{
			try
			{
				OracleConnection oleDbConnection = this.VerifyConnection(connection);
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					return mapper.PerformBatchLoad(instanceToLoad, (StormTableMappedAttribute)mapping, oleDbConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OracleDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Load instance of type [" + instanceToLoad.GetType().FullName + "].", e);
			}
		}

		public override void Persist<T>(T instanceToPersist, Storm.Attributes.ClassLevelMappedAttribute mapping, System.Data.IDbConnection connection, bool cascade)
		{
			try
			{
				OracleConnection oleDbConnection = this.VerifyConnection(connection);
				if (oleDbConnection == null)
					throw new StormPersistenceException("The database connection is not usable, not in the OPEN state, or not an OracleConnection.");
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					mapper.PerformPersist(instanceToPersist, (StormTableMappedAttribute)mapping, oleDbConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OracleDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Persist instance of type [" + instanceToPersist.GetType().FullName + "].", e);
			}
		}

		public override void BatchPersist<T>(List<T> listToPersist, System.Data.IDbConnection connection, bool cascade)
		{
			try
			{
				throw new NotImplementedException();
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Persist instance of type [" + typeof(T).FullName + "].", e);
			}
		}

		public override void Delete<T>(T instanceToDelete, Storm.Attributes.ClassLevelMappedAttribute mapping, System.Data.IDbConnection connection, bool cascade)
		{
			try
			{
				OracleConnection oleDbConnection = this.VerifyConnection(connection);
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					mapper.PerformDelete(instanceToDelete, (StormTableMappedAttribute)mapping, oleDbConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OracleDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Delete instance of type [" + instanceToDelete.GetType().FullName + "].", e);
			}
		}

		private OracleConnection VerifyConnection(IDbConnection connection)
		{
			OracleConnection oraConnection = connection as OracleConnection;
			if (oraConnection == null)
				throw new StormPersistenceException("The database connection is null or not an OracleConnection.");
			if (oraConnection.State != ConnectionState.Open)
				throw new StormPersistenceException("The database connection is not in the OPEN state.");
			return oraConnection;
		}
	}
}
