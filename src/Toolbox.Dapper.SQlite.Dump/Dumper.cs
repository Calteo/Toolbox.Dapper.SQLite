using System;
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Toolbox.Dapper.SQlite.Dump
{
	internal static class Dumper
	{
		public static void Write(string databasePath, string outputPath)
		{
			var outputDirectory = Path.GetDirectoryName(outputPath);

			if (!string.IsNullOrEmpty(outputDirectory))
			{
				Directory.CreateDirectory(outputDirectory);
			}

			var tempPath = outputPath + ".tmp";

			try
			{
				using var connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadOnly");

				connection.Open();

				using var writer = new StreamWriter(tempPath, false, new UTF8Encoding(false));

				WriteHeader(writer);

				WriteSchema(connection, writer);

				WriteData(connection, writer);

				WriteFooter(writer);

				writer.Flush();
				writer.Close();

				File.Move(tempPath, outputPath, true);
			}
			finally
			{
				if (File.Exists(tempPath))
				{
					File.Delete(tempPath);
				}
			}
		}

		private static void WriteHeader(StreamWriter writer)
		{
			writer.WriteLine("-- SQLite database dump");
			writer.WriteLine($"-- Generated: {DateTime.UtcNow:O}");
			writer.WriteLine();

			writer.WriteLine("PRAGMA foreign_keys=OFF;");
			writer.WriteLine("BEGIN TRANSACTION;");
			writer.WriteLine();
		}

		private static void WriteFooter(StreamWriter writer)
		{
			writer.WriteLine();
			writer.WriteLine("COMMIT;");
		}

		private static void WriteSchema(
			SqliteConnection connection,
			StreamWriter writer)
		{
			const string sql = """
            SELECT
                type,
                name,
                tbl_name,
                sql
            FROM sqlite_master
            WHERE sql IS NOT NULL
              AND name NOT LIKE 'sqlite_%'
            ORDER BY
                CASE type
                    WHEN 'table' THEN 1
                    WHEN 'index' THEN 2
                    WHEN 'trigger' THEN 3
                    WHEN 'view' THEN 4
                    ELSE 5
                END,
                name;
            """;

			using var command = connection.CreateCommand();
			command.CommandText = sql;

			using var reader = command.ExecuteReader();

			writer.WriteLine("--");
			writer.WriteLine("-- Schema");
			writer.WriteLine("--");
			writer.WriteLine();

			while (reader.Read())
			{
				var type = reader.GetString(0);
				var name = reader.GetString(1);
				var definition = reader.GetString(3);

				writer.WriteLine(
					$"-- {type}: {name}");

				writer.WriteLine(
					EnsureSemicolon(definition));

				writer.WriteLine();
			}
		}

		private static void WriteData(
			SqliteConnection connection,
			StreamWriter writer)
		{
			const string tableSql = """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table'
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name;
            """;

			using var tableCommand = connection.CreateCommand();
			tableCommand.CommandText = tableSql;

			using var tableReader = tableCommand.ExecuteReader();

			var tables = new List<string>();

			while (tableReader.Read())
			{
				tables.Add(tableReader.GetString(0));
			}

			writer.WriteLine("--");
			writer.WriteLine("-- Data");
			writer.WriteLine("--");
			writer.WriteLine();

			foreach (var table in tables)
			{
				WriteTableData(connection, writer, table);
			}
		}

		private static void WriteTableData(
			SqliteConnection connection,
			StreamWriter writer,
			string tableName)
		{
			var quotedTableName = QuoteIdentifier(tableName);

			using var command = connection.CreateCommand();

			command.CommandText =
				$"SELECT * FROM {quotedTableName};";

			using var reader = command.ExecuteReader();

			var columnCount = reader.FieldCount;

			if (columnCount == 0)
			{
				return;
			}

			writer.WriteLine(
				$"-- Data for table {tableName}");

			while (reader.Read())
			{
				var values = new string[columnCount];

				for (var i = 0; i < columnCount; i++)
				{
					values[i] = ToSqlLiteral(
						reader.GetValue(i));
				}

				writer.WriteLine(
					$"INSERT INTO {quotedTableName} VALUES ({string.Join(", ", values)});");
			}

			writer.WriteLine();
		}

		private static string ToSqlLiteral(object value)
		{
			if (value == DBNull.Value)
			{
				return "NULL";
			}

			return value switch
			{
				string s =>
					$"'{EscapeString(s)}'",

				char c =>
					$"'{EscapeString(c.ToString())}'",

				bool b =>
					b ? "1" : "0",

				byte b =>
					b.ToString(CultureInfo.InvariantCulture),

				sbyte b =>
					b.ToString(CultureInfo.InvariantCulture),

				short s =>
					s.ToString(CultureInfo.InvariantCulture),

				ushort s =>
					s.ToString(CultureInfo.InvariantCulture),

				int i =>
					i.ToString(CultureInfo.InvariantCulture),

				uint i =>
					i.ToString(CultureInfo.InvariantCulture),

				long l =>
					l.ToString(CultureInfo.InvariantCulture),

				ulong l =>
					l.ToString(CultureInfo.InvariantCulture),

				float f =>
					f.ToString(CultureInfo.InvariantCulture),

				double d =>
					d.ToString(CultureInfo.InvariantCulture),

				decimal d =>
					d.ToString(CultureInfo.InvariantCulture),

				byte[] bytes =>
					ToBlobLiteral(bytes),

				DateTime dt =>
					$"'{EscapeString(dt.ToString("O", CultureInfo.InvariantCulture))}'",

				DateTimeOffset dto =>
					$"'{EscapeString(dto.ToString("O", CultureInfo.InvariantCulture))}'",

				_ =>
					$"'{EscapeString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)}'"
			};
		}

		private static string ToBlobLiteral(byte[] bytes)
		{
			if (bytes.Length == 0)
			{
				return "X''";
			}

			return $"X'{Convert.ToHexString(bytes)}'";
		}

		private static string EscapeString(string value)
		{
			return value.Replace("'", "''");
		}

		private static string QuoteIdentifier(string identifier)
		{
			return $"\"{identifier.Replace("\"", "\"\"")}\"";
		}

		private static string EnsureSemicolon(string sql)
		{
			sql = sql.Trim();

			return sql.EndsWith(";")
				? sql
				: sql + ";";
		}
	}
}