using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite
{
	/// <summary>
	/// Interface for database models.
	/// </summary>
	public interface IDatabaseModel
	{		
		long Id { get; set; }
	}
}
