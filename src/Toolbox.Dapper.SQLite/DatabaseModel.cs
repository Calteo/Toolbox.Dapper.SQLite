using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite
{
	/// <summary>
	/// Base class for database models. 
	/// This class can be extended to represent specific database models and their properties.
	/// </summary>
	public class DatabaseModel : IDatabaseModel
	{
		[DbIdentity]
		public long Id { get; set; }
	}
}