using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite.Demo.Models
{
	internal class Person : DatabaseModel
	{
		public string Name { get; set; } = "";
		public int Age { get; set; }
		[DbColumn("Comment")]
		public string Description { get; set; } = "";
	}
}
