using System.Diagnostics;
using System.Xml.Linq;
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

			Trace.WriteLine("CURRENT SCHEMA");
			var schema = connection.Query("SELECT * FROM sqlite_schema");
			foreach (var row in schema)
			{
				Trace.WriteLine($"{row.type}/{row.name}/{row.tbl_name}");
			}
		}

		public void BulkInsertPeople(int count, bool rollback = false)
		{
			using var connection = GetConnection();
			using var transaction = connection.BeginTransaction();

			for (int i = 0; i < count; i++)
			{
				var p = new People { Name = $"Bulk#{i+1}", Age = 21+i };
				Peoples.Insert(p, transaction);
			}

			if (rollback) transaction.Rollback();
			else transaction.Commit();
		}
	}
}
