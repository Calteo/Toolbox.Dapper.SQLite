using System.Text;

namespace Toolbox.Dapper.SQLite.Factories
{
	public class SqliteFactory<T> : SqlFactory<T> where T : IDatabaseModel
	{
		public override string Select(string tableName)
		{
			var columns = Mappings.Select(m => 
				{
					if (m.Property.Name != m.ColumnName)
						return $"{Quote(m.ColumnName)} AS {Quote(m.Property.Name)}";
					else
						return Quote(m.ColumnName);
				});

			return $"SELECT {string.Join(", ", columns)} FROM {Quote(tableName)}";
		}

		public override string Insert(string tableName)
		{
			var valueMappings = Mappings.Where(m => !m.IsIdentity);
			var columns = valueMappings.Select(m => Quote(m.ColumnName));
			var parameters = valueMappings.Select(m => "@" + m.Property.Name);

			var builder = new StringBuilder();
			builder.AppendLine($"INSERT INTO {Quote(tableName)} ({string.Join(", ", columns)})");
			builder.AppendLine($"VALUES ({string.Join(", ", parameters)})");
			if (Identity != null)
				builder.AppendLine($"RETURNING {Quote(Identity.ColumnName)}");

			return builder.ToString();
		}

		public override string Update(string tableName)
		{
			var valueMappings = Mappings.Where(m => !m.IsIdentity);
			var assignments = valueMappings.Select(m => $"{Quote(m.ColumnName)} = @{m.Property.Name}");

			var builder = new StringBuilder();
			builder.AppendLine($"UPDATE {Quote(tableName)}");
			builder.AppendLine($"SET {string.Join(", ", assignments)}");
			AppendWhereIdentityOrKeys(builder);

			return builder.ToString() ;
		}

		public override string Delete(string tableName)
		{
			var builder = new StringBuilder();
			builder.AppendLine($"DELETE FROM {Quote(tableName)}");
			AppendWhereIdentityOrKeys(builder);
			return builder.ToString();
		}

		private void AppendWhereIdentityOrKeys(StringBuilder builder)
		{
			if (Identity != null)
			{
				builder.AppendLine($"WHERE {Quote(Identity.ColumnName)} = @{Identity.Property.Name}");
			}
			else
			{
				var whereKeys = Keys.Select(k => $"{Quote(k.ColumnName)}=@{k.Property.Name}");
				builder.AppendLine($"WHERE {string.Join(" AND ", whereKeys)}");
			}
		}
	}
}
