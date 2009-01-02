using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Storm.DataBinders.Oracle
{
	/// <summary>
	/// Extension methods for System.Data.IDbCommand
	/// </summary>
	internal static class DbCommandExtensions
	{
		/// <summary>
		/// Clear data that is not reused across command uses.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cmd"></param>
		internal static void ClearData<T>(this T cmd) where T : IDbCommand
		{
			foreach (DbParameter param in cmd.Parameters)
			{
				param.Value = null;
			}
			cmd.Connection = null;
			cmd.Transaction = null;
		}
	}
}
