using System.Diagnostics;
using Dapper;

namespace Toolbox.Dapper.SQLite.Test.Access
{
	internal class TestDatabase : Database
	{		
		public TestDatabase(string filename) : base(filename)
		{
			Peoples = AddTable<PeopleTable>();
		}

		public PeopleTable Peoples { get; }

		public bool CreateCalled { get; private set; }
		protected override void Create(VersionInfo version)
		{
			base.Create(version);

			CreateCalled = true;

			using var connection = GetConnection();

			Trace.WriteLine("SCHEMA");
			var schema = connection.Query("SELECT * FROM sqlite_schema");
			foreach (var row in schema)
			{
				Trace.WriteLine($"{row.type}/{row.name}/{row.tbl_name}");
			}

		}
	}
}
