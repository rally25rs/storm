using System;

namespace Storm.Attributes
{
    /// <summary>
    /// This attribute indicates that the decorated class
    ///  is to be mapped by Storm to a database table.
    /// The decorated class should contain properties that
    ///  are decorated with the [StormColumnMapped] attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public class StormTableMappedAttribute : System.Attribute
    {
        public string TableName { get; set; }

        public string DataBinderName { get; set; }

        /// <summary>
        /// Indicate that this class is to be mapped to a table.
        /// </summary>
        /// <param name="tableName">The name of the table to map to.</param>
        /// <param name="dataBinder">The fully qualified type name of the Data Binder to use.</param>
        public StormTableMappedAttribute(string tableName, string dataBinder)
        {
            this.TableName = tableName;
            this.DataBinderName = dataBinder;
        }
    }
}
