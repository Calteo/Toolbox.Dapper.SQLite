using System.Data;
using Dapper;
using Toolbox.Dapper.SQLite.Factories;

namespace Toolbox.Dapper.SQLite
{
	/// <summary>
	/// Represents a database table. This class can be extended to define specific tables and their properties.
	/// </summary>
	public partial class DatabaseTable<T> : IDatabaseTable where T : IDatabaseModel
	{
		/// <summary>
		/// Create a new instance of the <see cref="DatabaseTable{T}"/> class with the specified table name.
		/// </summary>
		/// <param name="tableName"></param>
		public DatabaseTable(string tableName)
		{
			TableName = tableName;
		}

		/// <summary>
		/// Name of the database table. This property is initialized through the constructor and is read-only.
		/// </summary>
		/// <remarks>
		/// Commands will use this property to replace a placeholder in the command text with the actual table name. This allows for dynamic table name usage in SQL commands.
		/// </remarks>
		public string TableName { get; }

		/// <summary>
		/// Get the type of the model associated with this database table. 
		/// </summary>
		/// <returns></returns>
		public Type GetModelType() => typeof(T);

		/// <inheritdoc />
		public virtual void Open()
		{
		}

		/// <inheritdoc />
		public virtual void Close()
		{
		}

		/// <summary>
		/// The database instance associated with this table. 
		/// This property is required and must be initialized when creating an instance of the DatabaseTable class.
		/// </summary>
		public required Database Database { get; init; }
		protected SqlFactory<T> Factory { get; set; } = new SqliteFactory<T>();

		private Dictionary<string, string> Commands { get; } = [];

		protected string? GetCommand(string name)
		{
			var key = $"{TableName}_{name}.sql";
			if (Commands.TryGetValue(key, out var command)) return command;

			command = GetType().TryGetRessourceString(name);
			if (command != null)
				Commands[key] = command;
			else 
			{
				command = Factory.Call(name, TableName);
			}

			return command;
		}

		public IEnumerable<T> Select()
		{
			var command = GetCommand(nameof(Select))
				?? throw new InvalidOperationException($"No command found for '{nameof(Select)}'.");

			using var connection = Database.GetConnection(true);
			return connection.Query<T>(command);
		}
		
		public IEnumerable<T> SelectWhere(string where, object? parameters = null)
		{
			var command = GetCommand(nameof(Select))
				?? throw new InvalidOperationException($"No command found for '{nameof(Select)}'.");

			var clause = Factory.ReplaceProperties(where);

			command += " WHERE " + clause;

			using var connection = Database.GetConnection(true);
			return connection.Query<T>(command, parameters);
		}


		public void Insert(T item, IDbTransaction? transaction = null)
		{
			var command = GetCommand(nameof(Insert))
				?? throw new InvalidOperationException($"No command found for '{nameof(Insert)}'.");

			using var connection = Database.GetConnection(true);
			var identity = connection.ExecuteScalar<long>(command, item, transaction);
			item.Id = identity;
		}

		public void Update(T item, IDbTransaction? transaction = null)
		{
			var command = GetCommand(nameof(Update))
				?? throw new InvalidOperationException($"No command found for '{nameof(Update)}'.");

			using var connection = Database.GetConnection(true);
			var affected = connection.Execute(command, item, transaction);
		}

		public void Delete(T item, IDbTransaction? transaction = null)
		{
			var command = GetCommand(nameof(Delete))
				?? throw new InvalidOperationException($"No command found for '{nameof(Delete)}'.");

			using var connection = Database.GetConnection(true);
			var affected = connection.Execute(command, item, transaction);
		}
	}
}
