using System;
using System.Reflection;
using System.Data;

namespace Storm.Attributes
{

    /// <summary>
    /// This attribute indicates that the decorated property
    ///  is to be mapped to a table column by Storm.
    /// </summary>
    public class StormColumnMappedAttribute : PropertyLevelMappedAttribute
    {
        public string ColumnName { get; set; }
		public bool PrimaryKey { get; set; }

        /// <summary>
        /// Indicates that this property is to be mapped to a table column.
        /// </summary>
        public StormColumnMappedAttribute() : base()
        {
        }
	}
}
