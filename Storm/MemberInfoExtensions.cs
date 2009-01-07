using System;
using System.Collections.Generic;
using System.Reflection;

namespace Storm
{
	public static class MemberInfoExtensions
	{
		private static Dictionary<MemberInfo, Dictionary<Type, object[]>> attribInfo = new Dictionary<MemberInfo, Dictionary<Type, object[]>>();
		public static T[] GetCachedAttributes<T>(this MemberInfo memberInfo, bool inherit) where T : Attribute
		{
			if (!attribInfo.ContainsKey(memberInfo))
			{
				attribInfo.Add(memberInfo, new Dictionary<Type, object[]>());
			}

			Dictionary<Type, object[]> attribsForTypes = attribInfo[memberInfo];
			if (!attribsForTypes.ContainsKey(typeof(T)))
			{
				object[] attributes = memberInfo.GetCustomAttributes(typeof(T), inherit);
				attribsForTypes.Add(typeof(T), attributes);
			}

			return (T[])attribsForTypes[typeof(T)];
		}
	}
}
