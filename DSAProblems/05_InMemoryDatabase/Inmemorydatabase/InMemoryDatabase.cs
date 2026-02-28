using Inmemorydatabase.Contracts;

namespace Inmemorydatabase;

/// <summary>
/// Simple in-memory database managing multiple tables.
/// </summary>
public class InMemoryDatabase : IDatabase
{
    private readonly Dictionary<string, Table> _tables = new(StringComparer.OrdinalIgnoreCase);

    public void CreateTable(string name, IEnumerable<ColumnDefinition> columns)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Table name is required", nameof(name));

        if (_tables.ContainsKey(name))
            throw new InvalidOperationException($"Table '{name}' already exists.");

        var table = new Table(name, columns);
        _tables[name] = table;
    }

    public void Insert(string tableName, IDictionary<string, object> values)
    {
        var table = GetTable(tableName);
        table.Insert(values);
    }

    public IEnumerable<IDictionary<string, object>> Select(
        string tableName,
        IRowFilter? filter = null,
        string? orderBy = null,
        bool desc = false)
    {
        var table = GetTable(tableName);
        Func<IDictionary<string, object>, bool>? predicate = null;

        if (filter != null)
        {
            predicate = filter.Evaluate;
        }

        return table.Select(predicate, orderBy, desc);
    }

    /// <summary>
    /// Indexer to access tables by name. Throws KeyNotFoundException if table doesn't exist.
    /// </summary>
    public Table this[string tableName]
    {
        get => GetTable(tableName);
    }

    private Table GetTable(string tableName)
    {
        if (!_tables.TryGetValue(tableName, out var table))
            throw new KeyNotFoundException($"Table '{tableName}' does not exist.");
        return table;
    }
}
