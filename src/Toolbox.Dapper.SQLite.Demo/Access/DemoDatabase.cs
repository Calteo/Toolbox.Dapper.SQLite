using Toolbox.Dapper.SQLite.Demo.Access.People;

namespace Toolbox.Dapper.SQLite.Demo.Access
{
	internal class DemoDatabase : Database
	{
		public DemoDatabase() : base("demo.db")
		{
			Peoples = AddTable<PeopleTable>();
		}

		public PeopleTable Peoples { get; }
	}
}
