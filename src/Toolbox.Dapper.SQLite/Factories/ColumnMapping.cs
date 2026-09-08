using System.Diagnostics;
using System.Reflection;
using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite.Factories
{
	[DebuggerDisplay("{PropertyName,nq} -> {ColumnName,nq}")]
	public class ColumnMapping
	{
		public ColumnMapping(PropertyInfo property)
		{
			Property = property;
			ColumnName = property.GetCustomAttribute<DbColumnAttribute>(true)?.Name ?? property.Name;
			IsIdentity = property.GetCustomAttribute<DbIdentityAttribute>(true) != null;

			var keyAttribute = property.GetCustomAttribute<DbKeyAttribute>(true);
			IsKey = keyAttribute != null;
			KeyOrder = keyAttribute != null ? keyAttribute.Order : 0;
		}

		public PropertyInfo	Property { get; }
		public string PropertyName => Property.Name;
		public string ColumnName { get; }
		public bool IsIdentity { get; }
		public bool IsKey { get; }
		public int KeyOrder { get; }
	}
}
