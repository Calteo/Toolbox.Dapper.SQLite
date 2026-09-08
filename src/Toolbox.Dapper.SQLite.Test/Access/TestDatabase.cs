namespace Toolbox.Dapper.SQLite.Test.Access
{
	internal class TestDatabase : Database
	{		
		public TestDatabase(string filename) : base(filename)
		{
			Peoples = AddTable<PeopleTable>();
		}

		public PeopleTable Peoples { get; }
	}
}
