namespace PgDeploy.Schema;

public sealed class TableDefinition
{
    public required string Schema { get; init; }
    public required string Name { get; init; }
    public List<ColumnDefinition> Columns { get; init; } = [];
    public PrimaryKeyDefinition? PrimaryKey { get; set; }
    public string FullName => $"{Schema}.{Name}";

    public ColumnDefinition? FindColumn(string name)
        => Columns.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
}
