namespace Toolbox.Dapper.SQLite.Attributes
{

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DbColumnAttribute(string columnName) : Attribute
	{
		/// <summary>
		/// Name of the column in the database table.
		/// </summary>
		public string Name { get; } = columnName;
	}
}
