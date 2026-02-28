namespace Inmemorydatabase.Contracts;

public interface IDatabase
{
    void CreateTable(string name, IEnumerable<ColumnDefinition> columns);
    void Insert(string tableName, IDictionary<string, object> values);
    IEnumerable<IDictionary<string, object>> Select(
        string tableName,
        IRowFilter? filter = null,
        string? orderBy = null,
        bool desc = false);
}
