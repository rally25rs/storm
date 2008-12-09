using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storm.DataBinders.OleDb
{
	/// <summary>
	/// Just a simple place to store info about a mapping.
	/// </summary>
	internal sealed class DataCache
	{
		private Dictionary<Type, DataCacheItem> cache = new Dictionary<Type, DataCacheItem>();

		internal DataCacheItem Get(Type t)
		{
			if (cache.ContainsKey(t))
				return cache[t];
			else
				return null;
		}

		internal void Add(Type t, DataCacheItem item)
		{
			cache.Add(t, item);
		}
	}
}
