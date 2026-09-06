using System.Reflection;
using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite.Factories
{
	public abstract class SqlFactory<T> where T : IDatabaseModel
	{
		public SqlFactory()
		{
			Mappings = [.. GetProperties().Select(p => new ColumnMapping(p))];

			var identities = Mappings.Where(m => m.IsIdentity).ToArray();
			if (identities.Length > 1)
				throw new InvalidOperationException($"Type {typeof(T).Name} had multiple identity properties.");

			Identity = identities.FirstOrDefault();			
			Keys = Mappings.Where(m =>m.IsIdentity).ToArray();

			if (Identity==null && Keys.Length==0)
				throw new InvalidOperationException($"Type {typeof(T).Name} has no identity or key properties.");
		}

		protected ColumnMapping[] Mappings { get; }
		protected ColumnMapping? Identity { get; }
		protected ColumnMapping[] Keys { get; }

		private IEnumerable<PropertyInfo> GetProperties()
		{
			return typeof(T)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.CanWrite)
				.Where(p => p.GetCustomAttribute<DbIgnoreAttribute>() == null);
		}

		protected virtual string Quote(string name) => name;

		abstract public string Select(string tableName);
		abstract public string Insert(string tableName);
		abstract public string Update(string tableName);
		abstract public string Delete(string tableName);

	}
}
