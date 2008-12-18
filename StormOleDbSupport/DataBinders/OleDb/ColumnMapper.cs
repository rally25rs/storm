using System;
using System.Collections.Generic;
using System.Data.OleDb;
using Storm.Attributes;

namespace Storm.DataBinders.OleDb
{
	internal sealed class ColumnMapper
	{
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
