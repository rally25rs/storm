using System;
using System.Reflection;

namespace Storm.Attributes
{
	public class StormProcedureMappedAttribute : ClassLevelMappedAttribute
	{
		// The name of the stored procedure.
		// Should be of the form: <owner>.<package>.<procedure> where owner and package are optional.
		// For example:
		//  "MyProcedure"  -  Map to "MyProcedure" that is not defined in a package.
		//  "PKG_MyPack.MyProcedure"  -  Map to "MyProcedure" defined in the "PKG_MyPack" package.
		//  "me.MyProcedure  -  Map to "MyProcedure" that is owned by the "me" schema.
		//  "me.PKG_MyPack.MyProcedure"  -  Map to "MyProcedure" defined in the "PKG_MyPack" package that is owned my the "me" schema.
		public string ProcedureName { get; set; }

		// private data loaded during validation
		private PropertyInfo[] MappedProperties { get; set; }

		public StormProcedureMappedAttribute()
			: base()
		{
		}

		internal override void ValidateMapping(Type decoratedType)
		{
			if (this.Validated)
				return;

			// base validation
			base.ValidateMapping(decoratedType);
			this.Validated = false;

			// must have a procedure name defined
			if (this.ProcedureName == null || this.ProcedureName.Length == 0)
				throw new StormConfigurationException("Invalid Procedure mapping on Type [" + decoratedType.FullName + "]. Must provide a procedure name.");

			foreach (PropertyInfo prop in decoratedType.GetProperties())
			{
				foreach (PropertyLevelMappedAttribute attrib in prop.GetCachedAttributes<PropertyLevelMappedAttribute>(true))
				{
					if (attrib.GetType() != typeof(StormParameterMappedAttribute) && attrib.GetType() != typeof(StormRelationMappedAttribute))
						throw new StormConfigurationException("Invalid mapping on Type [" + decoratedType.FullName + "], Property [" + prop.Name + "]. Procedure Mapped classes can not contain Properties mapped with this mapping type.");
				}
			}

			this.Validated = true;
		}

	}
}
