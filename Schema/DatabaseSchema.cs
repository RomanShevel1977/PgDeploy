namespace PgDeploy.Schema;

public sealed class DatabaseSchema
{
    public required string SchemaName { get; init; }
    public List<TableDefinition> Tables { get; init; } = [];

    public TableDefinition? FindTable(string name)
        => Tables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
}
