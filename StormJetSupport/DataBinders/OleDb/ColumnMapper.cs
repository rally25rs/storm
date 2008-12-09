using System;
using System.Collections.Generic;
using System.Data.OleDb;
using Storm.Attributes;

namespace Storm.DataBinders.OleDb
{
	internal sealed class ColumnMapper
	{
		internal void AddParameter<T>(T instanceToLoad, StormColumnMappedAttribute stormColumnMappedAttribute, OleDbCommand cmd)
		{
			if ((stormColumnMappedAttribute.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
			{
				var parm = cmd.CreateParameter();
				parm.Direction = System.Data.ParameterDirection.Input;
				parm.ParameterName = stormColumnMappedAttribute.ColumnName;
				parm.Value = stormColumnMappedAttribute.AttachedTo.GetValue(instanceToLoad, null);
				parm.OleDbType = DbTypeMap.ConvertTypeToDbType(stormColumnMappedAttribute.AttachedTo.PropertyType);
				cmd.Parameters.Add(parm);
			}
		}

		internal void MapResult<T>(T instanceToLoad, StormColumnMappedAttribute attribute, OleDbDataReader reader)
		{
			if((attribute.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
			{
				int col = reader.GetOrdinal(attribute.ColumnName);
				object dbValue = reader.GetValue(col);
				Type destType = attribute.AttachedTo.PropertyType;
				if(dbValue != null && dbValue != DBNull.Value)
					attribute.AttachedTo.SetValue(instanceToLoad, Convert.ChangeType(dbValue, destType), null);
			}
		}
	}
}
