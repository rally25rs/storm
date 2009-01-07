using System.Text;
using Oracle.DataAccess.Client;
using Storm.Attributes;

namespace Storm.DataBinders.Oracle
{
	/// <summary>
	/// Builds SQL queries for the DataBinder's select/insert/update/delete commands.
	/// </summary>
	internal sealed class CommandBuilder
	{
		internal static OracleCommand CreateSelectCommandForKeys(StormTableMappedAttribute mapping)
		{
			OracleCommand cmd = new OracleCommand();

			// loop through mapped properties and add them into the queries
			StringBuilder keysBuilder = new StringBuilder();
			StringBuilder columnsBuilder = new StringBuilder();
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormColumnMappedAttribute))
				{
					StormColumnMappedAttribute column = (StormColumnMappedAttribute)attrib;
					if ((column.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
					{
						if (column.PrimaryKey)
						{
							if (keysBuilder.Length > 0)
								keysBuilder.Append(" AND ");
							keysBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);

							OracleParameter param = cmd.CreateParameter();
							param.Direction = System.Data.ParameterDirection.Input;
							param.ParameterName = column.ColumnName;
							param.OracleDbType = DbTypeMap.ConvertTypeToDbType(column.AttachedTo.PropertyType);
							param.Size = DbTypeMap.GetDbTypeSize(param.OracleDbType);
							cmd.Parameters.Add(param);
						}
						else
						{
							if (columnsBuilder.Length > 0)
								columnsBuilder.Append(", ");
							columnsBuilder.Append(column.ColumnName);
						}
					}
				}
			}

			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "SELECT " + columnsBuilder.ToString() + " FROM " + mapping.TableName + " WHERE " + keysBuilder.ToString();

			return cmd;
		}

		internal static OracleCommand CreateSelectCommandForNonNull<T>(T instanceToLoad, StormTableMappedAttribute mapping)
		{
			OracleCommand cmd = new OracleCommand();

			// loop through mapped properties and add them into the queries
			StringBuilder keysBuilder = new StringBuilder();
			StringBuilder columnsBuilder = new StringBuilder();
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormColumnMappedAttribute))
				{
					StormColumnMappedAttribute column = (StormColumnMappedAttribute)attrib;
					if ((column.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
					{
						object value = column.AttachedTo.GetValue(instanceToLoad, null);
						if (value != null)
						{
							if (keysBuilder.Length > 0)
								keysBuilder.Append(" AND ");
							keysBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);

							OracleParameter param = cmd.CreateParameter();
							param.Direction = System.Data.ParameterDirection.Input;
							param.ParameterName = column.ColumnName;
							param.OracleDbType = DbTypeMap.ConvertTypeToDbType(column.AttachedTo.PropertyType);
							param.Size = DbTypeMap.GetDbTypeSize(param.OracleDbType);
							cmd.Parameters.Add(param);
						}
						if (columnsBuilder.Length > 0)
							columnsBuilder.Append(", ");
						columnsBuilder.Append(column.ColumnName);
					}
				}
			}

			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "SELECT " + columnsBuilder.ToString() + " FROM " + mapping.TableName;
			if (keysBuilder.Length > 0)
				cmd.CommandText += " WHERE " + keysBuilder.ToString();
			
			return cmd;
		}

		internal static OracleCommand CreateExistsCommand(StormTableMappedAttribute mapping)
		{
			OracleCommand cmd = new OracleCommand();

			// loop through mapped properties and add them into the queries
			StringBuilder keysBuilder = new StringBuilder();
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormColumnMappedAttribute))
				{
					StormColumnMappedAttribute column = (StormColumnMappedAttribute)attrib;
					if ((column.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load && column.PrimaryKey)
					{
						if (keysBuilder.Length > 0)
							keysBuilder.Append(" AND ");
						keysBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);

						OracleParameter param = cmd.CreateParameter();
						param.Direction = System.Data.ParameterDirection.Input;
						param.ParameterName = column.ColumnName;
						param.OracleDbType = DbTypeMap.ConvertTypeToDbType(column.AttachedTo.PropertyType);
						param.Size = DbTypeMap.GetDbTypeSize(param.OracleDbType);
						cmd.Parameters.Add(param);
					}
				}
			}

			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "SELECT count(*) FROM " + mapping.TableName + " WHERE " + keysBuilder.ToString();

			return cmd;
		}

		internal static OracleCommand CreateInsertCommand(StormTableMappedAttribute mapping)
		{
			OracleCommand cmd = new OracleCommand();

			// loop through mapped properties and add them into the queries
			StringBuilder columnsBuilder = new StringBuilder();
			StringBuilder valuesBuilder = new StringBuilder();
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormColumnMappedAttribute))
				{
					StormColumnMappedAttribute column = (StormColumnMappedAttribute)attrib;
					if ((column.SupressEvents & StormPersistenceEvents.Insert) != StormPersistenceEvents.Insert)
					{
						if(columnsBuilder.Length > 0)
							columnsBuilder.Append(", ");
						columnsBuilder.Append(column.ColumnName);
						if (valuesBuilder.Length > 0)
							valuesBuilder.Append(", ");
						valuesBuilder.Append(":").Append(column.ColumnName);

						OracleParameter param = cmd.CreateParameter();
						param.Direction = System.Data.ParameterDirection.Input;
						param.ParameterName = column.ColumnName;
						param.OracleDbType = DbTypeMap.ConvertTypeToDbType(column.AttachedTo.PropertyType);
						param.Size = DbTypeMap.GetDbTypeSize(param.OracleDbType);
						cmd.Parameters.Add(param);
					}
				}
			}

			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "INSERT INTO " + mapping.TableName + " (" + columnsBuilder.ToString() + ") VALUES (" + valuesBuilder.ToString() + ")";

			return cmd;
		}

		internal static OracleCommand CreateUpdateCommand(StormTableMappedAttribute mapping)
		{
			OracleCommand cmd = new OracleCommand();

			// loop through mapped properties and add them into the queries
			StringBuilder keysBuilder = new StringBuilder();
			StringBuilder columnsBuilder = new StringBuilder();
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormColumnMappedAttribute))
				{
					StormColumnMappedAttribute column = (StormColumnMappedAttribute)attrib;
					if ((column.SupressEvents & StormPersistenceEvents.Update) != StormPersistenceEvents.Update)
					{
						if (column.PrimaryKey)
						{
							if (keysBuilder.Length > 0)
								keysBuilder.Append(" AND ");
							keysBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);
						}
						else
						{
							if (columnsBuilder.Length > 0)
								columnsBuilder.Append(", ");
							columnsBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName); ;
						}

						OracleParameter param = cmd.CreateParameter();
						param.Direction = System.Data.ParameterDirection.Input;
						param.ParameterName = column.ColumnName;
						param.OracleDbType = DbTypeMap.ConvertTypeToDbType(column.AttachedTo.PropertyType);
						param.Size = DbTypeMap.GetDbTypeSize(param.OracleDbType);
						cmd.Parameters.Add(param);
					}
				}
			}

			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "UPDATE " + mapping.TableName + " SET " + columnsBuilder.ToString() + " WHERE " + keysBuilder.ToString();

			return cmd;
		}

		internal static OracleCommand CreateDeleteCommand(StormTableMappedAttribute mapping)
		{
			OracleCommand cmd = new OracleCommand();

			// loop through mapped properties and add them into the queries
			StringBuilder keysBuilder = new StringBuilder();
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormColumnMappedAttribute))
				{
					StormColumnMappedAttribute column = (StormColumnMappedAttribute)attrib;
					if ((column.SupressEvents & StormPersistenceEvents.Delete) != StormPersistenceEvents.Delete && column.PrimaryKey)
					{
						if (keysBuilder.Length > 0)
							keysBuilder.Append(" AND ");
						keysBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);

						OracleParameter param = cmd.CreateParameter();
						param.Direction = System.Data.ParameterDirection.Input;
						param.ParameterName = column.ColumnName;
						param.OracleDbType = DbTypeMap.ConvertTypeToDbType(column.AttachedTo.PropertyType);
						param.Size = DbTypeMap.GetDbTypeSize(param.OracleDbType);
						cmd.Parameters.Add(param);
					}
				}
			}

			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "DELETE FROM " + mapping.TableName + " WHERE " + keysBuilder.ToString();

			return cmd;
		}

		internal static OracleCommand CreateSelectCommandForProcedure(StormProcedureMappedAttribute mapping)
		{
			OracleCommand cmd = new OracleCommand();

			// loop through mapped properties and add them into the parameters
			foreach (var attrib in mapping.PropertyAttributes)
			{
				if (attrib.GetType() == typeof(StormParameterMappedAttribute))
				{
					StormParameterMappedAttribute paramAttrib = (StormParameterMappedAttribute)attrib;
					if ((paramAttrib.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
					{
						OracleParameter param = cmd.CreateParameter();
						param.Direction = paramAttrib.ParameterDirection;
						param.ParameterName = paramAttrib.ParameterName;
						param.OracleDbType = DbTypeMap.ConvertTypeToDbType(paramAttrib.AttachedTo.PropertyType);
						param.Size = DbTypeMap.GetDbTypeSize(param.OracleDbType);
						cmd.Parameters.Add(param);
					}
				}
			}

			cmd.CommandType = System.Data.CommandType.StoredProcedure;
			cmd.CommandText = mapping.ProcedureName;

			return cmd;
		}
	}
}
