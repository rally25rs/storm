using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Storm.Attributes;
using System.Data.OleDb;
using Storm.DataBinders.OleDb;
using System.Data;
using Storm.DataBinders.OleDb.Validation;

namespace Storm.DataBinders
{
	public class OleDbDataBinder : AbstractDataBinder
	{
		private DataCache dataCache = null;

		public OleDbDataBinder() : base()
		{
			this.dataCache = new DataCache();
		}

		public override void ValidateMapping(ClassLevelMappedAttribute mapping, IDbConnection connection)
		{
			if (mapping.DataBinderValidated)
				return;

			try
			{
				OleDbConnection oleDbConnection = this.VerifyConnection(connection);
				SchemaValidator validator = new SchemaValidator(oleDbConnection);
				validator.VerifyMapping(mapping);
				mapping.DataBinderValidated = true;
			}
			catch (Exception e)
			{
				throw new StormConfigurationException("OleDbDataBinder found an invalid mapping for type [" + mapping.AttachedTo.FullName + "].", e);
			}
		}

		public override void Load<T>(T instanceToLoad, Storm.Attributes.ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade)
		{
			try
			{
				OleDbConnection oleDbConnection = this.VerifyConnection(connection);
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					mapper.PerformLoad(instanceToLoad, (StormTableMappedAttribute)mapping, oleDbConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OleDbDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Load instance of type [" + instanceToLoad.GetType().FullName + "].", e);
			}
		}

		public override List<T> BatchLoad<T>(T instanceToLoad, Storm.Attributes.ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade)
		{
			try
			{
				OleDbConnection oleDbConnection = this.VerifyConnection(connection);
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					return mapper.PerformBatchLoad(instanceToLoad, (StormTableMappedAttribute)mapping, oleDbConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OleDbDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Load instance of type [" + instanceToLoad.GetType().FullName + "].", e);
			}
		}

		public override void Persist<T>(T instanceToPersist, Storm.Attributes.ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade)
		{
			try
			{
				OleDbConnection oleDbConnection = this.VerifyConnection(connection);
				if (oleDbConnection == null)
					throw new StormPersistenceException("The database connection is not usable, not in the OPEN state, or not an OleDbConnection.");
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					mapper.PerformPersist(instanceToPersist, (StormTableMappedAttribute)mapping, oleDbConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OleDbDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Persist instance of type [" + instanceToPersist.GetType().FullName + "].", e);
			}
		}

		public override void BatchPersist<T>(List<T> listToPersist, IDbConnection connection, bool cascade)
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

		public override void Delete<T>(T instanceToDelete, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade)
		{
			try
			{
				OleDbConnection oleDbConnection = this.VerifyConnection(connection);
				Type mappingType = mapping.GetType();
				if (mappingType == typeof(StormTableMappedAttribute))
				{
					var mapper = new TableMapper();
					mapper.PerformDelete(instanceToDelete, (StormTableMappedAttribute)mapping, oleDbConnection, dataCache);
				}
				else
				{
					throw new StormPersistenceException("Storm OleDbDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
				}
			}
			catch (Exception e)
			{
				throw new StormPersistenceException("Unable to Delete instance of type [" + instanceToDelete.GetType().FullName + "].", e);
			}
		}

		private OleDbConnection VerifyConnection(IDbConnection connection)
		{
			OleDbConnection oleDbConnection = connection as OleDbConnection;
			if (oleDbConnection == null)
				throw new StormPersistenceException("The database connection is null or not an OleDbConnection.");
			if (oleDbConnection.State != ConnectionState.Open)
				throw new StormPersistenceException("The database connection is not in the OPEN state.");
			return oleDbConnection;
		}
	}
}
