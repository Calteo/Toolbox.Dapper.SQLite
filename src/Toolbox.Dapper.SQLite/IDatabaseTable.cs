namespace Toolbox.Dapper.SQLite
{
	/// <summary>
	/// Interface for database tables. This interface can be implemented by classes that represent specific database tables.
	/// </summary>
	public interface IDatabaseTable
	{
		/// <summary>
		/// Return the type of the <see cref="IDatabaseModel"/> that the table represents. 
		/// </summary>
		/// <returns></returns>
		Type GetModelType();

		Database Database { get; init; }

		/// <summary>
		/// Called when the database is opened.
		/// </summary>
		void Open();
		/// <summary>
		/// Called when the database is closed.
		/// </summary>
		void Close();
	}
}
