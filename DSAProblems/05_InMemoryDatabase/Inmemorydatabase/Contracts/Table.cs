using System.Collections.ObjectModel;

namespace Inmemorydatabase.Contracts;

/// <summary>
/// Holds schema, rows, and primary-key index for a single table.
/// </summary>
public class Table
{
    public string Name { get; }
    public IReadOnlyList<ColumnDefinition> Schema { get; }

    private readonly List<Dictionary<string, object>> _rows = new();
    private readonly Dictionary<object, int> _primaryKeyIndex = new();
    private readonly string? _primaryKeyColumn;

    public Table(string name, IEnumerable<ColumnDefinition> columns)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        var columnList = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
        if (columnList.Count == 0)
            throw new ArgumentException("Table must have at least one column", nameof(columns));

        // Validate unique column names (case-insensitive to prevent duplicates like Id/id)
        var duplicate = columnList
            .GroupBy(c => c.ColumnName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
            throw new ArgumentException($"Duplicate column name: {duplicate.Key}");

        // Validate at most one primary key
        var primaryColumns = columnList.Where(c => c.IsPrimary).ToList();
        if (primaryColumns.Count > 1)
            throw new ArgumentException("Only one primary key column is allowed");

        _primaryKeyColumn = primaryColumns.FirstOrDefault()?.ColumnName;

        Schema = new ReadOnlyCollection<ColumnDefinition>(columnList);
    }

    /// <summary>
    /// Insert a row. Validates types and enforces primary key uniqueness.
    /// </summary>
    public void Insert(IDictionary<string, object> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in Schema)
        {
            if (!values.TryGetValue(column.ColumnName, out var value))
                throw new ArgumentException($"Missing value for column '{column.ColumnName}'");

            if (!IsValueCompatible(column.Type, value))
                throw new ArgumentException($"Value for column '{column.ColumnName}' is not of type {column.Type}");

            row[column.ColumnName] = value;
        }

        // Enforce primary key uniqueness
        if (_primaryKeyColumn != null)
        {
            var pkValue = row[_primaryKeyColumn];
            if (_primaryKeyIndex.ContainsKey(pkValue))
                throw new InvalidOperationException($"Duplicate primary key value '{pkValue}' for column '{_primaryKeyColumn}'");
        }

        _rows.Add(row);

        if (_primaryKeyColumn != null)
        {
            var pkValue = row[_primaryKeyColumn];
            _primaryKeyIndex[pkValue] = _rows.Count - 1;
        }
    }

    /// <summary>
    /// Select rows with optional filter and ordering.
    /// </summary>
    public IEnumerable<IDictionary<string, object>> Select(
        Func<IDictionary<string, object>, bool>? filter = null,
        string? orderBy = null,
        bool desc = false)
    {
        IEnumerable<Dictionary<string, object>> query = _rows;

        if (filter != null)
        {
            query = query.Where(r => filter(r));
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            if (!Schema.Any(c => string.Equals(c.ColumnName, orderBy, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Column '{orderBy}' does not exist in table '{Name}'", nameof(orderBy));

            query = desc
                ? query.OrderByDescending(r => r[orderBy!])
                : query.OrderBy(r => r[orderBy!]);
        }

        // Return copies to avoid external mutation of internal state
        foreach (var row in query)
        {
            yield return new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Fast O(1) lookup by primary key value. Returns empty if key not found.
    /// </summary>
    public IEnumerable<IDictionary<string, object>> SelectByPrimaryKey(object pkValue)
    {
        if (_primaryKeyColumn != null && _primaryKeyIndex.TryGetValue(pkValue, out var index))
        {
            yield return new Dictionary<string, object>(_rows[index], StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Try fast lookup by primary key if defined.
    /// </summary>
    public bool TryGetByPrimaryKey(object key, out IDictionary<string, object> row)
    {
        if (_primaryKeyColumn != null && _primaryKeyIndex.TryGetValue(key, out var index))
        {
            row = new Dictionary<string, object>(_rows[index], StringComparer.OrdinalIgnoreCase);
            return true;
        }

        row = new Dictionary<string, object>();
        return false;
    }

    private static bool IsValueCompatible(ColumnType columnType, object value)
    {
        return columnType switch
        {
            ColumnType.Integer => value is int,
            ColumnType.String => value is string,
            ColumnType.Boolean => value is bool,
            ColumnType.Double => value is double or float or decimal,
            ColumnType.DateTime => value is DateTime,
            _ => false
        };
    }
}
