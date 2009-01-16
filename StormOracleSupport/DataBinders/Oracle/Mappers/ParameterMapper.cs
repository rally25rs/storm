using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Storm.Attributes;

namespace Storm.DataBinders.Oracle.Mappers
{
	internal sealed class ParameterMapper
	{
		internal void MapResult(object instanceToLoad, StormParameterMappedAttribute attribute, OracleCommand cmd)
		{
			if (attribute.ParameterDirection == System.Data.ParameterDirection.Output || attribute.ParameterDirection == System.Data.ParameterDirection.InputOutput)
			{
				if ((attribute.SupressEvents & StormPersistenceEvents.Load) != StormPersistenceEvents.Load)
				{
					object dbValue = DbTypeMap.UnBoxOracleType(cmd.Parameters[attribute.ParameterName].Value);
					Type destType = attribute.AttachedTo.PropertyType;
					if (dbValue != null && dbValue != DBNull.Value)
						attribute.AttachedTo.SetValue(instanceToLoad, Convert.ChangeType(dbValue, destType), null);
					else
						attribute.AttachedTo.SetValue(instanceToLoad, null, null);
				}
			}
		}
	}
}
