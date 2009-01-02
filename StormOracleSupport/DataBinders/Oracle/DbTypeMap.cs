using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;

namespace Storm.DataBinders.Oracle
{
	internal sealed class DbTypeMap
	{
		private static Dictionary<Type, OracleDbType> TYPE_TO_DBTYPE_MAP;
		private static Dictionary<OracleDbType, Type> DBTYPE_TO_TYPE_MAP;

		internal static OracleDbType ConvertTypeToDbType(Type t)
		{
				if (TYPE_TO_DBTYPE_MAP == null)
					LoadDbTypeMaps();
				return TYPE_TO_DBTYPE_MAP[t];
		}

		internal static Type ConvertDbTypeToType(OracleDbType t)
		{
			if (DBTYPE_TO_TYPE_MAP == null)
				LoadDbTypeMaps();
			return DBTYPE_TO_TYPE_MAP[t];
		}

		private static void LoadDbTypeMaps()
		{
			if(TYPE_TO_DBTYPE_MAP == null)
			{
				TYPE_TO_DBTYPE_MAP = new Dictionary<Type, OracleDbType>();
				TYPE_TO_DBTYPE_MAP.Add(typeof(byte), OracleDbType.Byte);
				TYPE_TO_DBTYPE_MAP.Add(typeof(byte[]), OracleDbType.Blob);
				TYPE_TO_DBTYPE_MAP.Add(typeof(char), OracleDbType.Char);
				TYPE_TO_DBTYPE_MAP.Add(typeof(char[]), OracleDbType.Varchar2);
				TYPE_TO_DBTYPE_MAP.Add(typeof(DateTime), OracleDbType.Date);
				TYPE_TO_DBTYPE_MAP.Add(typeof(short), OracleDbType.Int16);
				TYPE_TO_DBTYPE_MAP.Add(typeof(int), OracleDbType.Int32);
				TYPE_TO_DBTYPE_MAP.Add(typeof(long), OracleDbType.Int64);
				TYPE_TO_DBTYPE_MAP.Add(typeof(float), OracleDbType.Single);
				TYPE_TO_DBTYPE_MAP.Add(typeof(double), OracleDbType.Double);
				TYPE_TO_DBTYPE_MAP.Add(typeof(decimal), OracleDbType.Decimal);
				TYPE_TO_DBTYPE_MAP.Add(typeof(string), OracleDbType.Varchar2);
			}
			if(DBTYPE_TO_TYPE_MAP == null)
			{
				DBTYPE_TO_TYPE_MAP = new Dictionary<OracleDbType, Type>();
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Blob, typeof(byte[]));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Byte, typeof(byte));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Char, typeof(char));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Clob, typeof(char[]));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Date, typeof(DateTime));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Decimal, typeof(decimal));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Double, typeof(double));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Int16, typeof(Int16));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Int32, typeof(Int32));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Int64, typeof(Int64));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Long, typeof(long));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.NVarchar2, typeof(string));
				DBTYPE_TO_TYPE_MAP.Add(OracleDbType.Varchar2, typeof(string));
			}
		}

		internal static int GetDbTypeSize(OracleDbType type)
		{
			switch (type)
			{
				case OracleDbType.Char:
				case OracleDbType.Byte:
					return 1;
				case OracleDbType.Int16:
					return 2;
				case OracleDbType.Int32:
					return 4;
				case OracleDbType.Int64:
					return 8;
				case OracleDbType.Varchar2:
					return 2048;
				default:
					return 0;
			}
		}
	}
}
