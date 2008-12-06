using System;
using System.Collections.Generic;
using Storm.DataBinders;

namespace Storm
{
    /// <summary>
    /// This is the main class for Storm.
    /// StormMapper is used to load and persist objects.
    /// </summary>
    public class StormMapper
    {
        /// <summary>
        /// This class should not be instantiated. Use static methods instead.
        /// TODO: Would be better to make a singleton and/or a factory.
        /// </summary>
        internal StormMapper()
        {
        }

        /// <summary>
        /// Take an instance of a Storm mapped class and load the
        ///  remaining data from the DB.
        /// </summary>
        /// <typeparam name="T">The type must be decorated with [StormTableMapped]</typeparam>
        /// <param name="instanceToLoad">An instance of the class to populate. All key properties must be populated.</param>
        /// <returns>Returns the loaded instance. Same as what was passed in.</returns>
        public static void Load<T>(T instanceToLoad)
		{
		}

        /// <summary>
        /// Take an instance of a Storm mapped class and load the
        ///   data into the DB. Performs insert or update as needed.
        /// </summary>
        /// <typeparam name="T">The type must be decorated with [StormTableMapped]</typeparam>
        /// <param name="instanceToPersist">An instance of the class to persist. All key properties must be populated.</param>
        public static void Persist<T>(T instanceToPersist)
		{
		}
}
