namespace Toolbox.Dapper.SQlite.Dump
{
	internal class Program
	{
		static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.Error.WriteLine("Usage: SqliteDump <database> <output.sql>");

				return 1;
			}

			var databasePath = Path.GetFullPath(args[0]);
			var outputPath = Path.GetFullPath(args[1]);

			if (!File.Exists(databasePath))
			{
				Console.Error.WriteLine($"SQLite database not found: {databasePath}");
				return 2;
			}

			try
			{
				Dumper.Write(databasePath, outputPath);

				Console.WriteLine($"SQLite dump generated: {outputPath}");

				return 0;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("SQLite dump failed.");
				Console.Error.WriteLine(ex);

				return 3;
			}
		}
	}
}