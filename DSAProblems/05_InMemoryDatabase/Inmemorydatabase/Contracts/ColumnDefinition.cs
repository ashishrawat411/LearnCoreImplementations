namespace Inmemorydatabase.Contracts;

/// <summary>
/// Immutable schema definition for a column.
/// </summary>
public record ColumnDefinition(
    ColumnType Type,
    string ColumnName,
    bool IsPrimary = false
);
