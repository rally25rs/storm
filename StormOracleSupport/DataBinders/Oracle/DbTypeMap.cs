using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Storm.DataBinders.Oracle
{
	internal sealed class DbTypeMap
	{
		private static Dictionary<Type, OracleDbType> TYPE_TO_DBTYPE_MAP;
		private static Dictionary<OracleDbType, Type> DBTYPE_TO_TYPE_MAP;
		private static Dictionary<string, OracleDbType> NAME_TO_DBTYPE_MAP;

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

		internal static OracleDbType ConvertNameToDbType(string name)
		{
			if (NAME_TO_DBTYPE_MAP == null)
				LoadDbTypeMaps();
			return NAME_TO_DBTYPE_MAP[name];
		}

		/// <summary>
		/// The need for this method is highly annoying.
		/// When Oracle sets its output parameters, the OracleParameter.Value property
		///  is set to an internal Oracle type, not its equivelant System type.
		///  For example, strings are returned as OracleString, DBNull is returned
		///  as OracleNull, blobs are returned as OracleBinary, etc...
		///  So these Oracle types need unboxed back to their normal system types.
		/// </summary>
		/// <param name="oracleType">Oracle type to unbox.</param>
		/// <returns></returns>
		internal static object UnBoxOracleType(object oracleType)
		{
			if (oracleType == null)
				return null;

			Type T = oracleType.GetType();
			if (T == typeof(OracleString))
			{
				if (((OracleString)oracleType).IsNull)
					return null;
				return ((OracleString)oracleType).Value;
			}
			else if (T == typeof(OracleDecimal))
			{
				if (((OracleDecimal)oracleType).IsNull)
					return null;
				return ((OracleDecimal)oracleType).Value;
			}
			else if (T == typeof(OracleBinary))
			{
				if (((OracleBinary)oracleType).IsNull)
					return null;
				return ((OracleBinary)oracleType).Value;
			}
			else if (T == typeof(OracleBlob))
			{
				if (((OracleBlob)oracleType).IsNull)
					return null;
				return ((OracleBlob)oracleType).Value;
			}
			else if (T == typeof(OracleDate))
			{
				if (((OracleDate)oracleType).IsNull)
					return null;
				return ((OracleDate)oracleType).Value;
			}
			else if (T == typeof(OracleTimeStamp))
			{
				if (((OracleTimeStamp)oracleType).IsNull)
					return null;
				return ((OracleTimeStamp)oracleType).Value;
			}
			else    // not sure how to handle these.
				return oracleType;
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
			if (NAME_TO_DBTYPE_MAP == null)
			{
				NAME_TO_DBTYPE_MAP = new Dictionary<string,OracleDbType>();
				NAME_TO_DBTYPE_MAP.Add("BLOB", OracleDbType.Blob);
				NAME_TO_DBTYPE_MAP.Add("BYTE", OracleDbType.Byte);
				NAME_TO_DBTYPE_MAP.Add("CHAR", OracleDbType.Char);
				NAME_TO_DBTYPE_MAP.Add("DATE", OracleDbType.Date);
				NAME_TO_DBTYPE_MAP.Add("DECIMAL", OracleDbType.Decimal);
				NAME_TO_DBTYPE_MAP.Add("DOUBLE", OracleDbType.Double);
				NAME_TO_DBTYPE_MAP.Add("NUMBER", OracleDbType.Int32);
				NAME_TO_DBTYPE_MAP.Add("VARCHAR", OracleDbType.Varchar2);
				NAME_TO_DBTYPE_MAP.Add("VARCHAR2", OracleDbType.Varchar2);
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
