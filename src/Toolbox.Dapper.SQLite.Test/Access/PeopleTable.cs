using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Dapper.SQLite.Test.Access
{
	internal class PeopleTable : DatabaseTable<People>
	{
		public PeopleTable() : base("People")
		{
		}
	}
}
