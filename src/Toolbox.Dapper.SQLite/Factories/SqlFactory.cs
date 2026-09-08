using System.Reflection;
using System.Text.RegularExpressions;
using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite.Factories
{
	public abstract partial class SqlFactory<T> where T : IDatabaseModel
	{
		public SqlFactory()
		{
			Mappings = GetProperties().Select(p => new ColumnMapping(p)).ToDictionary(m => m.PropertyName);

			var identities = Mappings.Values.Where(m => m.IsIdentity).ToArray();
			if (identities.Length > 1)
				throw new InvalidOperationException($"Type {typeof(T).Name} had multiple identity properties.");

			Identity = identities.FirstOrDefault();			
			Keys = Mappings.Values.Where(m => m.IsKey).OrderBy(m => m.KeyOrder).ToArray();

			if (Identity==null && Keys.Length==0)
				throw new InvalidOperationException($"Type {typeof(T).Name} has no identity or key properties.");

			Methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(m => m.ReturnType == typeof(string))
				.Where(m => { var p = m.GetParameters(); return p.Length == 1 && p[0].ParameterType == typeof(string); })
				.ToDictionary(m => m.Name);
		}

		protected Dictionary<string, ColumnMapping> Mappings { get; }
		protected ColumnMapping? Identity { get; }
		protected ColumnMapping[] Keys { get; }
		protected Dictionary<string, MethodInfo> Methods { get; } = [];

		private IEnumerable<PropertyInfo> GetProperties()
		{
			return typeof(T)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.CanWrite)
				.Where(p => p.GetCustomAttribute<DbIgnoreAttribute>() == null);
		}

		protected virtual string Quote(string name) => name;

		public string ReplaceProperties(string expression)
		{
			return ReplacePropertiesRegex().Replace(expression, ReplaceProperty);
		}

		private string ReplaceProperty(Match match)
		{
			var propertyName = match.Groups["name"].Value;
			if (!Mappings.TryGetValue(propertyName, out var mapping))
				throw new KeyNotFoundException($"Property for expression '{match.Value}' not found.");

			return Quote(mapping.ColumnName);
		}

		abstract public string Select(string tableName);
		abstract public string Insert(string tableName);
		abstract public string Update(string tableName);
		abstract public string Delete(string tableName);

		internal string? Call(string name, string tableName)
		{
			if (Methods.TryGetValue(name, out var method))
			{
				return (string?)method.Invoke(this, new object?[] { tableName });
			}
			return null;
		}

		[GeneratedRegex(@"(?<!\[)(?<property>\[(?<name>[^\[\]]+)\])(?!\])", RegexOptions.Compiled)]
		protected static partial Regex ReplacePropertiesRegex();
	}
}
