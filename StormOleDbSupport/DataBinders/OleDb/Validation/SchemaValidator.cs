using System;
using System.Collections.Generic;
using System.Text;
using Storm.Attributes;
using System.Data;
using System.Data.OleDb;

namespace Storm.DataBinders.OleDb.Validation
{
	/// <summary>
	/// Performs DB schema validation against a mapping.
	/// </summary>
	internal sealed class SchemaValidator
	{
		private OleDbConnection connection = null;

		internal SchemaValidator(OleDbConnection connection)
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
			DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, mapping.TableName, null });
			if (schema.Rows.Count == 0)
				throw new StormConfigurationException("The table named [" + mapping.TableName + "] does not exist.");

			object tableCatalog = schema.Rows[0]["TABLE_CATALOG"];
			object tableSchema = schema.Rows[0]["TABLE_SCHEMA"];
			object tableName = schema.Rows[0]["TABLE_NAME"];

			foreach (PropertyLevelMappedAttribute propMapping in mapping.PropertyAttributes)
			{
				if (propMapping is StormColumnMappedAttribute)
					ValidateColumnMapping(tableCatalog, tableSchema, tableName, (StormColumnMappedAttribute)propMapping);
				else
					throw new StormConfigurationException("Unhandled mapping type. Data Binder does not know how to handle the mapping [" + propMapping.GetType().FullName + "].");
			}
		}

		private void ValidateColumnMapping(object tableCatalog, object tableSchema, object tableName, StormColumnMappedAttribute mapping)
		{
			DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { tableCatalog, tableSchema, tableName, mapping.ColumnName });
			if (schema.Rows.Count == 0)
				throw new StormConfigurationException("The table named [" + tableName + "] does not contain a column named [" + mapping.ColumnName + "].");

			OleDbType columnType = (OleDbType)schema.Rows[0]["DATA_TYPE"];
			Type systemType = DbTypeMap.ConvertDbTypeToType(columnType);

			if (mapping.AttachedTo.PropertyType != systemType)
				throw new StormConfigurationException("The column [" + mapping.ColumnName + "] in table [" + tableName + "] is of type [" + systemType.FullName + "] but is mapped to a property with type [" + mapping.AttachedTo.PropertyType.FullName + "].");
		}
	}
}
