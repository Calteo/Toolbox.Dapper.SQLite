namespace Toolbox.Dapper.SQLite.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    sealed class DbKeyAttribute(int order) : Attribute
    {
        public int Order { get; } = order;
    } 
}
