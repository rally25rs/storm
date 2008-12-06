using System;

namespace Storm.Attributes
{
    [Flags]
    public enum StormPersistanceEvents
    {
        Load,
        Insert,
        Update,
        Delete,
    }

    /// <summary>
    /// This attribute indicates that the decorated property
    ///  is to be mapped to a table column by Storm.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    class StormColumnMappedAttribute : System.Attribute
    {
        public string ColumnName { get; set; }

        public StormPersistanceEvents SupressEvents { get; set; }

        /// <summary>
        /// Indicates that this property is to be mapped to a table column.
        /// </summary>
        /// <param name="columnName">The name of the column to map this property to.</param>
        public StormColumnMappedAttribute(string columnName)
        {
            this.ColumnName = columnName;
            this.SupressEvents = StormPersistanceEvents.Delete & StormPersistanceEvents.Insert; // this should really just return 0x0, or no flags set.
        }

        /// <summary>
        /// Indicates that this property is to be mapped to a table column.
        /// </summary>
        /// <param name="columnName">The name of the column to map this property to.</param>
        /// <param name="supressEvents">Persistance events that should NOT be run. Bitwise-OR the flags for multiple.</param>
        public StormColumnMappedAttribute(string columnName, StormPersistanceEvents supressEvents)
        {
            this.ColumnName = columnName;
            this.SupressEvents = supressEvents;
        }
    }
}
