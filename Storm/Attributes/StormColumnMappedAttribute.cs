using System;
using System.Reflection;

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

		// private data determined during validation
		private Type DatastoreType { get; set; }

        /// <summary>
        /// Indicates that this property is to be mapped to a table column.
        /// </summary>
        public StormColumnMappedAttribute() : base()
        {
        }
	}
}
