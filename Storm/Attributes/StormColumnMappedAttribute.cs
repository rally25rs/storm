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

		internal override void ValidateMapping(PropertyInfo decoratedProperty)
		{
			if (this.Validated)
				return;

			// base validation
			base.ValidateMapping(decoratedProperty);
			this.Validated = false;

			// must have a column name defined.
			// if no name specified, then reuse the Parameter name.
			if (this.ColumnName == null || this.ColumnName.Length == 0)
				this.ColumnName = decoratedProperty.Name;

			this.Validated = true;
		}
	}
}
