using PgDeploy.Schema;

namespace PgDeploy.Diff;

public sealed class SchemaDiff
{
    public List<TableDefinition> AddedTables { get; init; } = [];
    public List<TableChange> ChangedTables { get; init; } = [];
    public bool HasChanges => AddedTables.Count > 0 || ChangedTables.Any(x => x.HasChanges);
}
