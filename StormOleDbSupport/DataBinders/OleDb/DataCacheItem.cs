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
		internal string InsertQuery { get; set; }
		internal string UpdateQuery { get; set; }
		internal string DeleteQuery { get; set; }
		internal string ExistsQuery { get; set; }
	}
}
