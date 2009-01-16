using System.Data;

namespace Storm
{
	/// <summary>
	/// This interface can optionally be implemented by classed that are to be mapped by Storm.
	/// It provides methods simmilar to those in StormMapper, but can be called directly on
	///  the mapped classes, instead of using static methods in StormMapper.
	/// Note that classes should actually extend the abstract StormMapped class instead of
	///  directly implementing this interface.
	/// </summary>
	public interface IStormMapped
	{
		bool HasChangesToPersist { get; }

		void Load(IDbConnection connection);

		void Load(IDbConnection connection, bool cascade);

		void Persist(IDbConnection connection);

		void Persist(IDbConnection connection, bool cascade);

		void Delete(IDbConnection connection);

		void Delete(IDbConnection connection, bool cascade);

		/// <summary>
		/// Called by StormMapper after Load() completes.
		/// </summary>
		void StormLoaded();
	}
}
