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
		private bool mappingValidated = false;

		public OleDbDataBinder() : base()
		{
			this.dataCache = new DataCache();
		}

		public override void ValidateMapping(ClassLevelMappedAttribute mapping, IDbConnection connection)
		{
			if (this.mappingValidated)
				return;

			try
			{
				if (!(connection is OleDbConnection))
					throw new StormConfigurationException("DbConnection is not of type OleDbConnection. OleDbDataBinder can not use this connection.");
				SchemaValidator validator = new SchemaValidator((OleDbConnection)connection);
				validator.VerifyMapping(mapping);
				this.mappingValidated = true;
			}
			catch (Exception e)
			{
				throw new StormConfigurationException("OleDbDataBinder found an invalid mapping for type [" + mapping.AttachedTo.FullName + "].", e);
			}
		}

		public override void Load<T>(T instanceToLoad, Storm.Attributes.ClassLevelMappedAttribute mapping, RecordLookupMode lookupMode, IDbConnection connection, bool cascade)
		{
			OleDbConnection oleDbConnection = this.VerifyConnection(connection);
			if (oleDbConnection == null)
				throw new StormPersistenceException("The database connection is not usable, not in the OPEN state, or not an OleDbConnection.");
			Type mappingType = mapping.GetType();
			if(mappingType == typeof(StormTableMappedAttribute))
			{
				var mapper = new TableMapper();
				mapper.PerformLoad(instanceToLoad, (StormTableMappedAttribute)mapping, oleDbConnection, lookupMode, dataCache);
			}
			else
			{
				throw new StormPersistenceException("Storm JetDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
			}
		}

		public override void Persist<T>(T instanceToPersist, Storm.Attributes.ClassLevelMappedAttribute mapping, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		public override void BatchPersist<T>(List<T> listToPersist, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private OleDbConnection VerifyConnection(IDbConnection connection)
		{
			OleDbConnection oleDbConnection = connection as OleDbConnection;
			if (oleDbConnection == null)
				return null;
			if (oleDbConnection.State != ConnectionState.Open)
				return null;
			return oleDbConnection;
		}
	}
}
