using Toolbox.Dapper.SQLite.Demo.Models;

namespace Toolbox.Dapper.SQLite.Demo.Access.People
{
	internal class PeopleTable() : DatabaseTable<Person>("People")
	{
	}
}
