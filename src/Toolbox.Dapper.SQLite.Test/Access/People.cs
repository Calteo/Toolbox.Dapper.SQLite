using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite.Test.Access
{
	internal class People : DatabaseModel
	{
		public string Name { get; set; } = "";
		public int Age { get; set; }
		[DbColumn("Description")]
		public string Comment { get; set; } = "";
		[DbIgnore]
		public int Dummy { get; set; } = 42;
	}
}
