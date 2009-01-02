using System;
using Oracle.DataAccess.Client;

namespace Storm.DataBinders.Oracle
{
	/// <summary>
	/// Holds some info about a mapping that we want to cache.
	/// </summary>
	internal sealed class DataCacheItem
	{
		internal OracleCommand SelectCommand { get; set; }
		internal OracleCommand InsertCommand { get; set; }
		internal OracleCommand UpdateCommand { get; set; }
		internal OracleCommand DeleteCommand { get; set; }
		internal OracleCommand ExistsCommand { get; set; }
	}
}
