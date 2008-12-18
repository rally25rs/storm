using System;
using System.Reflection;

namespace Storm.Attributes
{
    /// <summary>
    /// This attribute indicates that the decorated class
    ///  is to be mapped by Storm to a database table.
    /// </summary>
    public class StormTableMappedAttribute : ClassLevelMappedAttribute
    {
        public string TableName { get; set; }

		// private data loaded during validation
		private PropertyInfo[] MappedProperties { get; set; }

        /// <summary>
        /// Indicate that this class is to be mapped to a table.
        /// </summary>
        public StormTableMappedAttribute() : base()
        {
        }

		internal override void ValidateMapping(Type decoratedType)
		{
			// base validation
			base.ValidateMapping(decoratedType);
			this.PreValidated = false;

			// must have a table nname defined
			if (this.TableName == null || this.TableName.Length == 0)
				throw new StormConfigurationException("Invalid Table mapping on Type [" + decoratedType.FullName +"]. Must provide a table name.");

			// currently TableMapped only knows how to handle column level mappings of: StormColumnMapped
			foreach (PropertyInfo prop in decoratedType.GetProperties())
			{
				foreach (PropertyLevelMappedAttribute attrib in this.GetPropertyLevelMappingAttributes(prop))
				{
					if (attrib.GetType() != typeof(StormColumnMappedAttribute))
						throw new StormConfigurationException("Invalid mapping on Type [" + decoratedType.FullName + "], Property [" + prop.Name + "]. Table Mapped classes can not contain Properties mapped with this mapping type."); 
				}
			}

			this.PreValidated = true;
		}
    }
}
