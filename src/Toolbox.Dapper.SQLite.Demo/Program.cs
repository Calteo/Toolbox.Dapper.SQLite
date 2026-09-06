using Toolbox.Dapper.SQLite.Demo.Access;
using Toolbox.Dapper.SQLite.Demo.Models;

namespace Toolbox.Dapper.SQLite.Demo
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var database = new DemoDatabase();
			database.Open();

			Console.WriteLine($"Database version: {database.Version.Id}, changed at {database.Version.ChangedAt}, comment: {database.Version.Comment}");

			var found = database.Peoples.Select().ToArray();

			foreach (var item in found)
			{
				Console.WriteLine($"[{item.Id}] - {item.Name} - Age {item.Age}");
			}			

			if (found.Length > 2)
			{
				Console.WriteLine($"Deleting {found[0].Id}");
				database.Peoples.Delete(found[0]);				
			}

			var person = new Person
			{
				Name = $"Test User - {DateTime.Now}",
				Age = new Random().Next(1, 80)
			};

			database.Peoples.Insert(person);
			Console.WriteLine($"Inserted id {person.Id}");

			if (found.Length > 1)
			{
				Console.WriteLine($"Updating {found[1].Id} -> Age = 42");
				found[1].Age = 42;
				database.Peoples.Update(found[1]);
			}


			database.Close();
		}
	}
}
