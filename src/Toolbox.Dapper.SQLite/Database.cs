using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Toolbox.Dapper.SQLite
{
	/// <summary>
	/// Represents a database connection and provides methods for executing queries and commands against the database.
	/// </summary>
	public class Database
	{
		/// <summary>
		/// Create new instance of <see cref="Database"/>.
		/// </summary>
		/// <param name="filename"></param>
		public Database(string filename)
		{
			Filename = filename;
			Connection = GetConnection();
		}

		/// <summary>
		/// Filename of the SQLite database file.
		/// </summary>
		public string Filename { get; }

		/// <summary>
		/// Current version number 
		/// </summary>
		protected virtual int CurrentVersion => 1;

		private SqliteConnection Connection { get; } 

		internal SqliteConnection GetConnection(bool open = false)
		{
			var connection = new SqliteConnection($"Data Source={Filename}");
			if (open) connection.Open(); 
			return connection;
		}

		private VersionInfo? _version;
		/// <summary>
		/// Actual version of the database schema, represented by the Version table. This property is only available after the database has been opened using the Open() method. If accessed before opening the database, an InvalidOperationException will be thrown.
		/// </summary>
		public VersionInfo Version => _version 
			?? throw new InvalidOperationException("Database is not open. Call Open() before accessing the Version property.");

		/// <summary>
		/// Database tables that are registered with the database. 
		/// This dictionary maps the type of the table to its corresponding IDatabaseTable instance. 
		/// </summary>
		private Dictionary<Type, IDatabaseTable> _tables = new Dictionary<Type, IDatabaseTable>();

		public T AddTable<T>(T? table = default) where T : IDatabaseTable
		{
			if (table == null) 
				table = Activator.CreateInstance<T>()
					?? throw new ArgumentNullException(nameof(table));
			
			table.GetType().GetProperty(nameof(IDatabaseTable.Database))!.SetValue(table, this);

			var modelType = table.GetModelType();
			_tables[modelType] = table;

			if (Connection.State == System.Data.ConnectionState.Open)
			{
				table.Open();
			}

			return table;
		}

		/// <summary>
		/// Opens the database connection and initializes the database schema if it does not exist. 
		/// If the database already exists, it checks the current version and performs an upgrade if necessary. 
		/// After opening the database, the Version property will be available to access the current version information.
		/// </summary>
		public void Open()
		{
			Connection.Open();

			var exists = Connection.ExecuteScalar<bool>("SELECT 1 FROM sqlite_schema WHERE type = 'table' AND name = 'Version'");
			_version = exists ? Upgrade() : Create();

			foreach (var table in _tables.Values) table.Open();
		}

		/// <summary>
		/// Closes the database connection and releases any resources associated with it. 
		/// After calling this method, the Version property will no longer be available until the database is opened again using the Open() method.
		/// </summary>
		public void Close()
		{
			foreach (var table in _tables.Values) table.Close();
			Connection.Close();
			_version = null;
		}

		private VersionInfo? Create()
		{
			using var stream = GetType().GetRessourceStream(GetType().Name + ".db");
			var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

			using (var file = File.Create(path))
			{
				stream.CopyToAsync(file);
			}

			using var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly;");
			connection.Open();

			var createScript = ExtractSchema(connection);
			Connection.Execute(createScript);

			var version = new VersionInfo
			{
				Id = CurrentVersion,
				ChangedAt = DateTime.UtcNow,
				Comment = "Initial version"
			};

			Connection.Execute("INSERT INTO Version (Id, ChangedAt, Comment) VALUES (@Id, @ChangedAt, @Comment)", version);

			return version;
		}

		private string ExtractSchema(SqliteConnection connection)
		{
			var script = new StringBuilder();

			// -----------------------------------------------------------------
			// PRAGMA settings
			// -----------------------------------------------------------------

			var foreignKeys = connection.ExecuteScalar<long>("PRAGMA foreign_keys;");
			var userVersion = connection.ExecuteScalar<long>("PRAGMA user_version;");
			var applicationId = connection.ExecuteScalar<long>(				"PRAGMA application_id;");

			script.AppendLine("PRAGMA foreign_keys = ON;");
			script.AppendLine($"PRAGMA user_version = {userVersion};");
			script.AppendLine($"PRAGMA application_id = {applicationId};");
			script.AppendLine();

			script.AppendLine("BEGIN TRANSACTION;");
			script.AppendLine();

			// -----------------------------------------------------------------
			// Tables
			// -----------------------------------------------------------------

			var tables = connection.Query<string>("""
            SELECT sql
            FROM sqlite_master
            WHERE type = 'table'
              AND sql IS NOT NULL
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name;
            """);

			script.AppendLine("-- Tables");
			script.AppendLine();

			foreach (var sql in tables)
			{
				script.AppendLine(sql.TrimEnd() + ";");
				script.AppendLine();
			}

			// -----------------------------------------------------------------
			// Indexes
			// -----------------------------------------------------------------

			var indexes = connection.Query<string>("""
            SELECT sql
            FROM sqlite_master
            WHERE type = 'index'
              AND sql IS NOT NULL
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name;
            """);

			script.AppendLine("-- Indexes");
			script.AppendLine();

			foreach (var sql in indexes)
			{
				script.AppendLine(sql.TrimEnd() + ";");
				script.AppendLine();
			}

			// -----------------------------------------------------------------
			// Triggers
			// -----------------------------------------------------------------

			var triggers = connection.Query<string>("""
            SELECT sql
            FROM sqlite_master
            WHERE type = 'trigger'
              AND sql IS NOT NULL
            ORDER BY name;
            """);

			script.AppendLine("-- Triggers");
			script.AppendLine();

			foreach (var sql in triggers)
			{
				script.AppendLine(sql.TrimEnd() + ";");
				script.AppendLine();
			}

			// -----------------------------------------------------------------
			// Views
			// -----------------------------------------------------------------

			var views = connection.Query<string>("""
            SELECT sql
            FROM sqlite_master
            WHERE type = 'view'
              AND sql IS NOT NULL
            ORDER BY name;
            """);

			script.AppendLine("-- Views");
			script.AppendLine();

			foreach (var sql in views)
			{
				script.AppendLine(sql.TrimEnd() + ";");
				script.AppendLine();
			}

			script.AppendLine("COMMIT;");
			return script.ToString();
		}

		private VersionInfo? Upgrade()
		{
			var version = Connection.QueryFirst<VersionInfo>("SELECT * FROM Version ORDER BY Id DESC LIMIT 1");

			if (version.Id != CurrentVersion)
			{
				UpgradeFrom(version.Id);
				version.Comment = $"Upgraded to version {CurrentVersion} from version {version.Id}";
				version.Id = CurrentVersion;
				version.ChangedAt = DateTime.UtcNow;
				Connection.Execute("INSERT INTO Version (Id, ChangedAt, Comment) VALUES (@Id, @ChangedAt, @Comment)", version);
			}

			return version;
		}

		protected virtual void UpgradeFrom(int version)
		{
			throw new NotImplementedException($"Upgrade from version {version} is not implemented.");
		}

		/// <summary>
		/// Version if the database schema, represented by the Version table.
		/// </summary>
		public class VersionInfo
		{
			public int Id { get; internal set; }
			public DateTime ChangedAt { get; internal set; }
			public string Comment { get; internal set; } = "";	
		}
	}
}
