using Dapper;

namespace Toolbox.Dapper.SQLite.Test.Access
{
	internal class PeopleTable : DatabaseTable<TestDatabase, People>
	{
		public PeopleTable() : base("People")
		{
		}

		public People? SelectHugo()
		{
			var sql = GetCommand("SelectHugo");
			if (string.IsNullOrEmpty(sql)) throw new InvalidOperationException("Command SelectHugo not found");

			return GetConnection().QuerySingleOrDefault<People>(sql);
		}
	}
}
