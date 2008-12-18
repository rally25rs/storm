using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace Storm.DataBinders.OleDb
{
	internal sealed class DbTypeMap
	{
		private static Dictionary<Type, OleDbType> TYPE_TO_DBTYPE_MAP;
		private static Dictionary<OleDbType, Type> DBTYPE_TO_TYPE_MAP;

		internal static OleDbType ConvertTypeToDbType(Type t)
		{
				if (TYPE_TO_DBTYPE_MAP == null)
					LoadDbTypeMaps();
				return TYPE_TO_DBTYPE_MAP[t];
		}

		internal static Type ConvertDbTypeToType(OleDbType t)
		{
			if (DBTYPE_TO_TYPE_MAP == null)
				LoadDbTypeMaps();
			return DBTYPE_TO_TYPE_MAP[t];
		}

		private static void LoadDbTypeMaps()
		{
			if(TYPE_TO_DBTYPE_MAP == null)
			{
				TYPE_TO_DBTYPE_MAP = new Dictionary<Type, OleDbType>();
				TYPE_TO_DBTYPE_MAP.Add(typeof(byte), OleDbType.UnsignedTinyInt);
				TYPE_TO_DBTYPE_MAP.Add(typeof(byte[]), OleDbType.Binary);
				TYPE_TO_DBTYPE_MAP.Add(typeof(char), OleDbType.Char);
				TYPE_TO_DBTYPE_MAP.Add(typeof(char[]), OleDbType.VarChar);
				TYPE_TO_DBTYPE_MAP.Add(typeof(DateTime), OleDbType.Date);
				TYPE_TO_DBTYPE_MAP.Add(typeof(TimeSpan), OleDbType.DBTime);
				TYPE_TO_DBTYPE_MAP.Add(typeof(short), OleDbType.SmallInt);
				TYPE_TO_DBTYPE_MAP.Add(typeof(int), OleDbType.Integer);
				TYPE_TO_DBTYPE_MAP.Add(typeof(long), OleDbType.BigInt);
				TYPE_TO_DBTYPE_MAP.Add(typeof(float), OleDbType.Single);
				TYPE_TO_DBTYPE_MAP.Add(typeof(double), OleDbType.Double);
				TYPE_TO_DBTYPE_MAP.Add(typeof(decimal), OleDbType.Decimal);
				TYPE_TO_DBTYPE_MAP.Add(typeof(string), OleDbType.VarChar);
			}
			if(DBTYPE_TO_TYPE_MAP == null)
			{
				// taken from: http://msdn.microsoft.com/en-us/library/system.data.oledb.oledbtype.aspx
				DBTYPE_TO_TYPE_MAP = new Dictionary<OleDbType, Type>();
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.BigInt, typeof(Int64));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Binary, typeof(byte[]));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Boolean, typeof(bool));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.BSTR, typeof(string));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Char, typeof(string));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Currency, typeof(decimal));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Date, typeof(DateTime));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.DBDate, typeof(DateTime));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.DBTime, typeof(TimeSpan));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.DBTimeStamp, typeof(DateTime));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Decimal, typeof(decimal));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Double, typeof(double));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Filetime, typeof(DateTime));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Integer, typeof(int));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.LongVarBinary, typeof(byte));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.LongVarChar, typeof(string));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.LongVarWChar, typeof(string));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Numeric, typeof(decimal));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.Single, typeof(Single));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.SmallInt, typeof(Int16));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.TinyInt, typeof(sbyte));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.UnsignedBigInt, typeof(UInt64));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.UnsignedInt, typeof(UInt32));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.UnsignedSmallInt, typeof(UInt16));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.UnsignedTinyInt, typeof(byte));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.VarBinary, typeof(byte[]));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.VarChar, typeof(string));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.VarNumeric, typeof(decimal));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.VarWChar, typeof(string));
				DBTYPE_TO_TYPE_MAP.Add(OleDbType.WChar, typeof(string));
			}
		}

		internal static int GetDbTypeSize(OleDbType oleDbType)
		{
			switch (oleDbType)
			{
				case OleDbType.Boolean:
					return 1;
				case OleDbType.SmallInt:
					return 2;
				case OleDbType.Integer:
					return 4;
				case OleDbType.BigInt:
					return 8;
				case OleDbType.Decimal:
					return 16;
				case OleDbType.VarChar:
				case OleDbType.Char:
				case OleDbType.Binary:
					return 2048;
				default:
					return 0;
			}
		}
	}
}
