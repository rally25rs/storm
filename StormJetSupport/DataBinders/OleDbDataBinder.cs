using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Storm.Attributes;
using System.Data.OleDb;
using Storm.DataBinders.OleDb;

namespace Storm.DataBinders
{
	public class OleDbDataBinder : AbstractDataBinder
	{
		private OleDbConnection connection = null;
		private DataCache dataCache = null;

		public OleDbDataBinder(OleDbConnection connection) : base()
		{
			if (connection == null)
				throw new ArgumentException("The connection can not be null.", "connection");
			this.connection = connection;
			this.dataCache = new DataCache();
		}

		public override void Load<T>(T instanceToLoad, Storm.Attributes.ClassLevelMappedAttribute mapping)
		{
			connection.Open();
			Type mappingType = mapping.GetType();
			if(mappingType == typeof(StormTableMappedAttribute))
			{
				var mapper = new TableMapper();
				mapper.PerformLoad(instanceToLoad, (StormTableMappedAttribute)mapping, connection, dataCache);
			}
			else
			{
				throw new StormPersistenceException("Storm JetDataBinder does not support the mapping [" + mapping.GetType().FullName + "].");
			}
			connection.Close();
		}

		public override void Persist<T>(T instanceToPersist, Storm.Attributes.ClassLevelMappedAttribute mapping)
		{
			throw new NotImplementedException();
		}

		public override void BatchPersist<T>(List<T> listToPersist)
		{
			throw new NotImplementedException();
		}
	}
}
