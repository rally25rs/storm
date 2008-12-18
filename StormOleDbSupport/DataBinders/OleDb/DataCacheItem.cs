using System;
using System.Data.OleDb;

namespace Storm.DataBinders.OleDb
{
	/// <summary>
	/// Holds some info about a mapping that we want to cache.
	/// </summary>
	internal sealed class DataCacheItem
	{
		internal OleDbCommand SelectCommand { get; set; }
		internal OleDbCommand InsertCommand { get; set; }
		internal OleDbCommand UpdateCommand { get; set; }
		internal OleDbCommand DeleteCommand { get; set; }
		internal OleDbCommand ExistsCommand { get; set; }
	}
}
