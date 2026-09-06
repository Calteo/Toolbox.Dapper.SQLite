namespace Toolbox.Dapper.SQLite.Demo.Models
{
	internal class Person : DatabaseModel
	{
		public string Name { get; set; } = "";
		public int Age { get; set; }
	}
}
