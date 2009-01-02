using System;
using System.Data;
using Oracle.DataAccess.Client;
using Storm.Attributes;

namespace Storm.DataBinders.Oracle.Validation
{
	/// <summary>
	/// Performs DB schema validation against a mapping.
	/// </summary>
	internal sealed class SchemaValidator
	{
		private OracleConnection connection = null;

		internal SchemaValidator(OracleConnection connection)
		{
			this.connection = connection;
		}

		internal void VerifyMapping(ClassLevelMappedAttribute mapping)
		{
			if (mapping is StormTableMappedAttribute)
				ValidateTableMapping((StormTableMappedAttribute)mapping);
			else
				throw new StormConfigurationException("Unhandled mapping type. Data Binder does not know how to handle the mapping [" + mapping.GetType().FullName + "].");
		}

		private void ValidateTableMapping(StormTableMappedAttribute mapping)
		{
			DataTable schema = connection.GetSchema("TABLES", new string[] { null, null, mapping.TableName, null });
			if (schema.Rows.Count == 0)
				throw new StormConfigurationException("The table named [" + mapping.TableName + "] does not exist.");

			string tableCatalog = schema.Rows[0]["TABLE_CATALOG"].ToString();
			string tableSchema = schema.Rows[0]["TABLE_SCHEMA"].ToString();
			string tableName = schema.Rows[0]["TABLE_NAME"].ToString();

			foreach (PropertyLevelMappedAttribute propMapping in mapping.PropertyAttributes)
			{
				if (propMapping is StormColumnMappedAttribute)
					ValidateColumnMapping(tableCatalog, tableSchema, tableName, (StormColumnMappedAttribute)propMapping);
				else
					throw new StormConfigurationException("Unhandled mapping type. Data Binder does not know how to handle the mapping [" + propMapping.GetType().FullName + "].");
			}
		}

		private void ValidateColumnMapping(string tableCatalog, string tableSchema, string tableName, StormColumnMappedAttribute mapping)
		{
			DataTable schema = connection.GetSchema("COLUMNS", new string[] { tableCatalog, tableSchema, tableName, mapping.ColumnName });
			if (schema.Rows.Count == 0)
				throw new StormConfigurationException("The table named [" + tableName + "] does not contain a column named [" + mapping.ColumnName + "].");

			OracleDbType columnType = (OracleDbType)schema.Rows[0]["DATA_TYPE"];
			Type systemType = DbTypeMap.ConvertDbTypeToType(columnType);

			if (mapping.AttachedTo.PropertyType != systemType)
				throw new StormConfigurationException("The column [" + mapping.ColumnName + "] in table [" + tableName + "] is of type [" + systemType.FullName + "] but is mapped to a property with type [" + mapping.AttachedTo.PropertyType.FullName + "].");
		}
	}
}
