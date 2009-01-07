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
			else if (mapping is StormProcedureMappedAttribute)
				ValidateProcedureMapping((StormProcedureMappedAttribute)mapping);
			else
				throw new StormConfigurationException("Unhandled mapping type. Data Binder does not know how to handle the mapping [" + mapping.GetType().FullName + "].");
		}

		private void ValidateProcedureMapping(StormProcedureMappedAttribute mapping)
		{
			// break the procedure name into its 3 parts: owner.package.procedure
			string owner = null, package = null, procedure = null;
			string[] parts = mapping.ProcedureName.ToUpper().Split(".".ToCharArray());
			bool tryOwnAndPack = false;
			switch (parts.Length)
			{
				case(1):
					procedure = parts[0];
					break;
				case(2):	// this could be "owner.procedure" or "package.procedure".
					owner = parts[0];
					package = parts[0];
					procedure = parts[1];
					tryOwnAndPack = true;
					break;
				case(3):
					owner = parts[0];
					package = parts[1];
					procedure = parts[2];
					break;
				default:
					procedure = mapping.ProcedureName;	// probably invalid, but take their word for it.
					break;
			}

			// the Oracle ALL_ARGUMENTS table seems to contain everything we need for functions and procedures both in and not in packages. Very nice!
			OracleCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT ARGUMENT_NAME, DATA_TYPE, IN_OUT FROM ALL_ARGUMENTS WHERE";
			if (procedure != null)
			{
				cmd.CommandText += " OBJECT_NAME = :procedure";
				cmd.Parameters.Add("procedure", procedure);
			}
			else
			{
				throw new StormConfigurationException("ProcedureMapped atribute attached to [" + mapping.AttachedTo.Name + "] does not have a procedure name.");
			}
			if (!tryOwnAndPack)
			{
				if (owner != null)
				{
					cmd.CommandText += " AND OWNER = :owner";
					cmd.Parameters.Add("owner", owner);
				}
				if (package != null)
				{
					cmd.CommandText += " AND PACKAGE_NAME = :package";
					cmd.Parameters.Add("package", package);
				}
			}
			else
			{
				cmd.CommandText += " AND ((OWNER = :owner AND PACKAGE_NAME IS NULL) OR ( OWNER IS NULL AND PACKAGE_NAME = :package))";
				cmd.Parameters.Add("owner", owner);
				cmd.Parameters.Add("package", package);
			}

			DataTable procedureArgumentsTable = new DataTable();
			using (var reader = cmd.ExecuteReader())
			{
				procedureArgumentsTable.Load(reader);
				reader.Close();
			}
			cmd.Dispose();

			// if we got something back, then the procedure exists.
			if (procedureArgumentsTable.Rows.Count == 0)
				throw new StormConfigurationException("Procedure named [" + mapping.ProcedureName + "] does not exist.");

			// validate each parameter.
			foreach (PropertyLevelMappedAttribute propMapping in mapping.PropertyAttributes)
			{
				if (propMapping is StormParameterMappedAttribute)
					ValidateParameterMapping(procedureArgumentsTable, (StormParameterMappedAttribute)propMapping);
				else if (!(propMapping is StormRelationMappedAttribute))
					throw new StormConfigurationException("Unhandled mapping type. Data Binder does not know how to handle the mapping [" + propMapping.GetType().FullName + "].");
			}
		}

		private void ValidateParameterMapping(DataTable procedureArgumentsTable, StormParameterMappedAttribute mapping)
		{
			// assuming the columns, in order, are: {ARGUMENT_NAME, DATA_TYPE, IN_OUT}
			string paramName = mapping.ParameterName.ToUpper();	// the names retreived from Oracle are all upper case.
			foreach (DataRow dr in procedureArgumentsTable.Rows)
			{
				if ((string)dr[0] == paramName)
				{
					// check data type
					OracleDbType columnType = DbTypeMap.ConvertNameToDbType((string)dr[1]);
					Type systemType = DbTypeMap.ConvertDbTypeToType(columnType);
					if (mapping.AttachedTo.PropertyType != systemType)
						throw new StormConfigurationException("The parameter named [" + mapping.ParameterName + "] is of type [" + systemType.FullName + "] but is mapped to a property with type [" + mapping.AttachedTo.PropertyType.FullName + "].");

					// check direction
					if (mapping.ParameterDirection == ParameterDirection.Input)
					{
						if ((string)dr[2] != "IN")
							throw new StormConfigurationException("Invalid mapping. The parameter named [" + mapping.ParameterName + "] is marked as an Input parameter, but the DB thinks it is [" + (string)dr[3] + "].");
					}
					else if (mapping.ParameterDirection == ParameterDirection.InputOutput)
					{
						if ((string)dr[2] != "IN/OUT")
							throw new StormConfigurationException("Invalid mapping. The parameter named [" + mapping.ParameterName + "] is marked as an Input/Output parameter, but the DB thinks it is [" + (string)dr[3] + "].");
					}
					else if (mapping.ParameterDirection == ParameterDirection.Output)
					{
						if ((string)dr[2] != "OUT")
							throw new StormConfigurationException("Invalid mapping. The parameter named [" + mapping.ParameterName + "] is marked as an Output parameter, but the DB thinks it is [" + (string)dr[3] + "].");
					}

					// valid mapping
					return;
				}
			}
			throw new StormConfigurationException("Invalid mapping. No parameter named [" + mapping.ParameterName + "] exists.");
		}

		private void ValidateTableMapping(StormTableMappedAttribute mapping)
		{
			string tableName = null, owner = null;
			string[] parts = mapping.TableName.ToUpper().Split('.');
			if (parts.Length == 2)
			{
				owner = parts[0];
				tableName = parts[1];
			}
			else
				tableName = parts[0];

			DataTable schema = connection.GetSchema("Tables", new string[] { owner, tableName });
			if (schema.Rows.Count == 0)
				throw new StormConfigurationException("The table named [" + mapping.TableName + "] does not exist.");

			tableName = schema.Rows[0]["TABLE_NAME"].ToString();

			foreach (PropertyLevelMappedAttribute propMapping in mapping.PropertyAttributes)
			{
				if (propMapping is StormColumnMappedAttribute)
					ValidateColumnMapping(tableName, (StormColumnMappedAttribute)propMapping);
				else if (!(propMapping is StormRelationMappedAttribute))
					throw new StormConfigurationException("Unhandled mapping type. Data Binder does not know how to handle the mapping [" + propMapping.GetType().FullName + "].");
			}
		}

		private void ValidateColumnMapping(string tableName, StormColumnMappedAttribute mapping)
		{
			DataTable schema = connection.GetSchema("Columns", new string[] { null, tableName, mapping.ColumnName.ToUpper() });
			if (schema.Rows.Count == 0)
				throw new StormConfigurationException("The table named [" + tableName + "] does not contain a column named [" + mapping.ColumnName + "].");

			OracleDbType columnType = DbTypeMap.ConvertNameToDbType(schema.Rows[0]["DATATYPE"].ToString());
			Type systemType = DbTypeMap.ConvertDbTypeToType(columnType);

			if (mapping.AttachedTo.PropertyType != systemType)
				throw new StormConfigurationException("The column [" + mapping.ColumnName + "] in table [" + tableName + "] is of type [" + systemType.FullName + "] but is mapped to a property with type [" + mapping.AttachedTo.PropertyType.FullName + "].");
		}
	}
}
