using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Toolbox.Dapper.SQLite.Test.Access;

namespace Toolbox.Dapper.SQLite.Test
{
	[TestClass]
	public sealed class DatabaseTest
	{
		private string _databaseFile = "";

		[TestInitialize]
		public void Initialize()
		{
			_databaseFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

			var database = new TestDatabase(_databaseFile);

			Trace.WriteLine($"Init database {_databaseFile}");
		}

		[TestCleanup]
		public void Cleanup()
		{
			SqliteConnection.ClearAllPools();

			if (File.Exists(_databaseFile))
				File.Delete(_databaseFile);
		}

		[TestMethod]
		public void OpenCloseDatabase()
		{
			using var cut = new TestDatabase(_databaseFile);
			
			cut.Open();

			Trace.WriteLine($"Version: {cut.Version.Id} - {cut.Version.Comment} at {cut.Version.ChangedAt}");

			Assert.IsNotNull(cut.Version);
			Assert.AreEqual(1, cut.Version.Id);
			Assert.IsTrue(cut.CreateCalled);

			cut.Close();
			Assert.Throws<InvalidOperationException>(() => cut.Version);
		}

		[TestMethod]
		public void ReOpenDatabase()
		{
			using (var db = new TestDatabase(_databaseFile))
			{
				db.Open();
				db.Close();
			}

			using var cut = new TestDatabase(_databaseFile);
			cut.Open();

			Assert.IsNotNull(cut.Version);
			Assert.AreEqual(1, cut.Version.Id);
			Assert.IsFalse(cut.CreateCalled);

			cut.Close();
			Assert.Throws<InvalidOperationException>(() => cut.Version);
		}



		[TestMethod]
		public void InsertSelectUpdateDelete()
		{
			using var cut = new TestDatabase(_databaseFile);
			cut.Open();

			const string Name = "Hugo";
			const int Age = 1;
			const string Comment = "Some Comment";
			const int Dummy = 959;

			// INSERT
			var p = new People { Name = Name, Age = Age, Comment = Comment, Dummy = Dummy };

			cut.Peoples.Insert(p);

			Assert.AreNotEqual(0, p.Id);
			Assert.AreEqual(Name, p.Name);
			Assert.AreEqual(Age, p.Age);
			Assert.AreEqual(Comment, p.Comment);
			Assert.AreEqual(Dummy, p.Dummy);

			// SELECT
			var selected = cut.Peoples.Select();
			Assert.HasCount(1, selected);
			var first = selected.First();
			Assert.AreEqual(p.Id, first.Id);
			Assert.AreEqual(p.Name, first.Name);
			Assert.AreEqual(p.Age, first.Age);
			Assert.AreEqual(p.Comment, first.Comment);
			Assert.AreEqual(42, first.Dummy);

			// UPDATE
			first.Comment = "Updated";
			cut.Peoples.Update(first);

			// VERIFY
			var updated = cut.Peoples.SelectWhere("[Id] = @Id", first);
			Assert.HasCount(1, updated);
			var changed = updated.First();
			Assert.AreEqual("Updated", changed.Comment);

			// DELETE
			cut.Peoples.Delete(changed);

			// VERIFY
			var all = cut.Peoples.Select();
			Assert.HasCount(0, all);
		}
	}
}