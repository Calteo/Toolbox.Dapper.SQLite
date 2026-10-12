using System.Data;
using Dapper;
using Toolbox.Dapper.SQLite.Factories;

namespace Toolbox.Dapper.SQLite
{
	/// <summary>
	/// Represents a database table. This class can be extended to define specific tables and their properties.
	/// </summary>
	public partial class DatabaseTable<TD, TM> : IDatabaseTable<TD> where TM : IDatabaseModel where TD : Database
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
		/// The database containing this table
		/// </summary>
		public required TD Database { get; init; }

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
		public Type GetModelType() => typeof(TM);

		/// <inheritdoc />
		public virtual void Open()
		{
		}

		/// <inheritdoc />
		public virtual void Close()
		{
		}

		protected IDbConnection GetConnection() => Database.GetConnection();

		protected SqlFactory<TM> Factory { get; set; } = new SqliteFactory<TM>();

		private Dictionary<string, string> Commands { get; } = [];

		protected string? GetCommand(string name)
		{
			var key = $"{GetType().Name}_{name}.sql";
			if (Commands.TryGetValue(key, out var command)) return command;

			command = GetType().TryGetRessourceString(key);
			if (command != null)
				Commands[key] = command;
			else
			{
				command = Factory.Call(name, TableName);
			}

			return command;
		}

		public IEnumerable<TM> Select()
		{
			var command = GetCommand(nameof(Select))
				?? throw new InvalidOperationException($"No command found for '{nameof(Select)}'.");

			using var connection = GetConnection();
			return connection.Query<TM>(command);
		}

		public IEnumerable<TM> SelectWhere(string where, object? parameters = null)
		{
			var command = GetCommand(nameof(Select))
				?? throw new InvalidOperationException($"No command found for '{nameof(Select)}'.");

			var clause = Factory.ReplaceProperties(where);

			command += " WHERE " + clause;

			using var connection = GetConnection();
			return connection.Query<TM>(command, parameters);
		}

		public void Insert(TM item, IDbTransaction? transaction = null)
		{
			var command = GetCommand(nameof(Insert))
				?? throw new InvalidOperationException($"No command found for '{nameof(Insert)}'.");

			var connection = transaction?.Connection ?? GetConnection();
			try
			{
				var identity = connection.ExecuteScalar<long>(command, item, transaction);
				item.Id = identity;
			}
			finally
			{
				if (transaction is null) connection.Dispose();
			}
		}

		public void Update(TM item, IDbTransaction? transaction = null)
		{
			var command = GetCommand(nameof(Update))
				?? throw new InvalidOperationException($"No command found for '{nameof(Update)}'.");

			var connection = transaction?.Connection ?? GetConnection();
			try
			{
				var affected = connection.Execute(command, item, transaction);
			}
			finally
			{
				if (transaction is null) connection.Dispose();
			}
		}

		public void Delete(TM item, IDbTransaction? transaction = null)
		{
			var command = GetCommand(nameof(Delete))
				?? throw new InvalidOperationException($"No command found for '{nameof(Delete)}'.");

			var connection = transaction?.Connection ?? GetConnection();
			try
			{
				connection.Execute(command, item, transaction);
			}
			finally
			{
				if (transaction is null) connection.Dispose();
			}
		}

		public void DeleteWhere(string where, IDbTransaction? transaction = null)
		{
			var command = GetCommand(nameof(DeleteWhere))
				?? throw new InvalidOperationException($"No command found for '{nameof(DeleteWhere)}'.");

			if (!string.IsNullOrWhiteSpace(where))
			{
				var clause = Factory.ReplaceProperties(where);
				command += " WHERE " + clause;
			}

			var connection = transaction?.Connection ?? GetConnection();
			try
			{
				connection.Execute(command, transaction);
			}
			finally
			{
				if (transaction is null) connection.Dispose();
			}
		}

	}
}
