using System;
using System.Collections.Generic;
using Storm.Attributes;
using System.Data.Common;
using System.Data;

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
		/// Verify that the given mapping is valid, as far as the DataBinder is concerned.
		/// This is a good place to validate schema!
		/// </summary>
		/// <param name="mapping">The mapping to validate.</param>
		/// <exception cref="StormConfigurationException">If mapping is invalid.</exception>
		void ValidateMapping(ClassLevelMappedAttribute mapping, IDbConnection connection);

		/// <summary>
		/// Take an instance of a Storm mapped class and load the
		///  remaining data from the DB.
		/// </summary>
		/// <typeparam name="T">The type decorated with ClassLevelMappedAttribute</typeparam>
		/// <param name="instanceToLoad">An instance of the class to populate. All key properties must be populated.</param>
		/// <param name="mapping">The mapping attribute attached to the instance.</param>
		/// <param name="lookupMode">The lookup mode to use.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the load (recursive load). If true, any properties that are mapped to other mapped objects will also be loaded.</param>
		/// <returns>Returns the loaded instance. Same as what was passed in.</returns>
		void Load<T>(T instanceToLoad, ClassLevelMappedAttribute mapping, RecordLookupMode lookupMode, IDbConnection connection, bool cascade);

        /// <summary>
        /// Take an instance of a Storm mapped class and load the
        ///   data into the DB. Performs insert or update as needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceToPersist">An instance of the class to persist. All key properties must be populated.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the update (recursive update). If true, any properties that are mapped to other mapped objects will also be updated.</param>
		void Persist<T>(T instanceToPersist, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);

		/// <summary>
		/// Take a list of instances of a Storm mapped class and load
		///  the data into the DB. Performs insert or update as needed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="listToPersist">List of items to persist.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the update (recursive update). If true, any properties that are mapped to other mapped objects will also be updated.</param>
		void BatchPersist<T>(List<T> listToPersist, IDbConnection connection, bool cascade);

		/// <summary>
		/// Take an instance of a Storm mapped class and delete the data from the DB.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instanceToDelete">An instance of the class to delete. All key properties must be populated.</param>
		/// <param name="connection">The Database Connection to run against.</param>
		/// <param name="cascade">Whether or not to cascade the delete (recursive delete). If true, any properties that are mapped to other mapped objects will also be deleted.</param>
		void Delete<T>(T instanceToDelete, ClassLevelMappedAttribute mapping, IDbConnection connection, bool cascade);
	}
}
