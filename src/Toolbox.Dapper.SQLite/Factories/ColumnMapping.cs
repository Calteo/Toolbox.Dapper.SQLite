using System.Reflection;
using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite.Factories
{
	public class ColumnMapping
	{
		public ColumnMapping(PropertyInfo property)
		{
			Property = property;
			ColumnName = property.GetCustomAttribute<DbColumnAttribute>(true)?.Name ?? property.Name;
			IsIdentity = property.GetCustomAttribute<DbIdentityAttribute>(true) != null;
			IsKey = property.GetCustomAttribute<DbKeyAttribute>(true) != null;
		}

		public PropertyInfo	Property { get; }
		public string ColumnName { get; }
		public bool IsIdentity { get; }
		public bool IsKey { get; }
	}
}
