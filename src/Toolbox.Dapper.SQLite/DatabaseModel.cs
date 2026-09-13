using Toolbox.ComponentModel;
using Toolbox.Dapper.SQLite.Attributes;

namespace Toolbox.Dapper.SQLite
{
	/// <summary>
	/// Base class for database models. 
	/// This class can be extended to represent specific database models and their properties.
	/// </summary>
	public class DatabaseModel : NotifyObject, IDatabaseModel
	{
		#region Id
		private long _id;
		[DbIdentity]
		public long Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}
		#endregion
	}
}