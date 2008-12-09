using System;
using System.Collections.Generic;

namespace Storm.Mappings
{
	internal sealed class ValidatedMappingCache
	{
		//Holds the cached mappings. Keyed by Type.
		private static Dictionary<Type, ValidatedClassLevelMapping> cachedMappings;

		private ValidatedMappingCache()
		{
		}

		/// <summary>
		/// Get the validated mapping information for a given Type.
		/// </summary>
		/// <param name="t">The Type to get mapping info for.</param>
		/// <returns>The validated mapping object, or null if none found.</returns>
		internal static ValidatedClassLevelMapping Get(Type t)
		{
			if (cachedMappings == null)
				return null;
			lock (cachedMappings)
			{
				return cachedMappings[t];
			}
		}

		/// <summary>
		/// Add a mapping to the cache.
		/// </summary>
		/// <param name="t">The Type that this mapping belongs to.</param>
		/// <param name="mapping">The mapping to cache.</param>
		internal static void Add(Type t, ValidatedClassLevelMapping mapping)
		{
			if (cachedMappings == null)
				cachedMappings = new Dictionary<Type, ValidatedClassLevelMapping>();
			lock (cachedMappings)
			{
				cachedMappings.Add(t, mapping);
			}
		}

		/// <summary>
		/// Remove a cached mapping.
		/// </summary>
		/// <param name="t">The Type to remove the mapping for.</param>
		internal static void Remove(Type t)
		{
			if (cachedMappings != null)
			{
				lock (cachedMappings)
				{
					cachedMappings.Remove(t);
				}
			}
		}

		internal static void Remove(ValidatedClassLevelMapping mapping)
		{
			if(cachedMappings != null)
			{
				lock (cachedMappings)
				{
					Type key = null;
					using (var keyEnum = cachedMappings.Keys.GetEnumerator())
					{
						while (keyEnum.MoveNext())
						{
							key = keyEnum.Current;
							var val = cachedMappings[key];
							if (val == mapping)
								break;
						}
					}
					if (key != null)
						cachedMappings.Remove(key);
				}
			}
		}

		/// <summary>
		/// Clear any cached mappings.
		/// </summary>
		internal static void Clear()
		{
			if (cachedMappings != null)
			{
				lock (cachedMappings)
				{
					cachedMappings.Clear();
				}
			}
		}
	}
}
