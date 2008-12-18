using System;
using System.Data.OleDb;
using System.Text;
using Storm.Attributes;
using System.Collections.Generic;

namespace Storm.DataBinders.OleDb
{
	/// <summary>
	/// Builds SQL queries for the OleDbDataBinder's select/insert/update/delete commands.
	/// </summary>
	internal sealed class CommandBuilder
	{
		internal static OleDbCommand CreateSelectCommandForKeys(StormTableMappedAttribute mapping)
		{
			OleDbCommand cmd = new OleDbCommand();

			// loop through mapped properties and add them into the queries
			List<string> keys = new List<string>();
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
							keys.Add(column.ColumnName);
							if (keysBuilder.Length > 0)
								keysBuilder.Append(" AND ");
							keysBuilder.Append(column.ColumnName).Append(" = :").Append(column.ColumnName);

							OleDbParameter param = cmd.CreateParameter();
							param.Direction = System.Data.ParameterDirection.Input;
							param.ParameterName = column.ColumnName;
							param.OleDbType = DbTypeMap.ConvertTypeToDbType(column.AttachedTo.PropertyType);
							param.Size = DbTypeMap.GetDbTypeSize(param.OleDbType);
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

			string sql = "SELECT " + columnsBuilder.ToString() + " FROM " + mapping.TableName + " WHERE " + keysBuilder.ToString();

			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = sql;

			return cmd;
		}

		internal static OleDbCommand CreateSelectCommandForNonNull(StormTableMappedAttribute mapping)
		{
			OleDbCommand cmd = new OleDbCommand();
			return cmd;
		}
	}
}
