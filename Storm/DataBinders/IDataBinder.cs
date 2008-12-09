using System;
using System.Collections.Generic;
using Storm.Attributes;

namespace Storm.DataBinders
{
    /// <summary>
    /// This is the main interface for a Data Binder class.
    /// Data Binders do the work of loading and persisting
    ///  classes that are decorated with the [StormTableMapped]
    ///  attribute. Basically, they contain the CRUD operations.
    /// The Storm library provides some default implementations,
    ///  but exposes this interface incase users want to create
    ///  their own custom binders. However, it is recommended
    ///  that custom binders extend AbstractDataBinder instead
    ///  of implement this interface directly.
    /// </summary>
    public interface IDataBinder
    {
		/// <summary>
		/// Take an instance of a Storm mapped class and load the
		///  remaining data from the DB.
		/// </summary>
		/// <typeparam name="T">The type decorated with ClassLevelMappedAttribute</typeparam>
		/// <param name="instanceToLoad">An instance of the class to populate. All key properties must be populated.</param>
		/// <param name="mapping">The mapping attribute attached to the instance.</param>
		/// <returns>Returns the loaded instance. Same as what was passed in.</returns>
		void Load<T>(T instanceToLoad, ClassLevelMappedAttribute mapping);

        /// <summary>
        /// Take an instance of a Storm mapped class and load the
        ///   data into the DB. Performs insert or update as needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceToPersist">An instance of the class to persist. All key properties must be populated.</param>
        void Persist<T>(T instanceToPersist, ClassLevelMappedAttribute mapping);

		/// <summary>
		/// Take a list of instances of a Storm mapped class and load
		///  the data into the DB. Performs insert or update as needed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="listToPersist">List of items to persist.</param>
		void BatchPersist<T>(List<T> listToPersist);
    }
}
