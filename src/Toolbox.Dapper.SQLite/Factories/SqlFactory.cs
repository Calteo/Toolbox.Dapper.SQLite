using System.Reflection;
using System.Text.RegularExpressions;
using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite.Factories
{
	/// <summary>
	/// Generic SQL statement factory
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract partial class SqlFactory<T> where T : IDatabaseModel
	{
		/// <summary>
		/// Create a new instance of the <see cref="SqlFactory{T}"/> class.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
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

		/// <summary>
		/// Replaces properties with their column names.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Creates a SELECT statement with further clauses
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		abstract public string Select(string tableName);
		/// <summary>
		/// Creates an INSERT statement for a single <see cref="DatabaseModel"/>.
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		abstract public string Insert(string tableName);
		/// <summary>
		/// Creates an UPDATE statement for a single <see cref="DatabaseModel"/>.
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		abstract public string Update(string tableName);
		/// <summary>
		/// Creates an DELETE statement for a single <see cref="DatabaseModel"/>.
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		abstract public string Delete(string tableName);
		/// <summary>
		/// Creates an DELETE statement without WHERE clause.
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		/// <remarks>The <see cref="DatabaseTable{TD, TM}.DeleteWhere"/> uses this to attach a custom WHERE clause.</remarks>
		abstract public string DeleteWhere(string tableName);

		internal string? Call(string name, string tableName)
		{
			if (Methods.TryGetValue(name, out var method))
			{
				return (string?)method.Invoke(this, [tableName]);
			}
			return null;
		}

		[GeneratedRegex(@"(?<!\[)(?<property>\[(?<name>[^\[\]]+)\])(?!\])", RegexOptions.Compiled)]
		protected static partial Regex ReplacePropertiesRegex();
	}
}
