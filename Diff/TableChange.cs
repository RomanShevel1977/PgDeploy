using PgDeploy.Schema;

namespace PgDeploy.Diff;

public sealed class TableChange
{
    public required TableDefinition SourceTable { get; init; }
    public required TableDefinition TargetTable { get; init; }
    public List<ColumnDefinition> AddedColumns { get; init; } = [];
    public List<ColumnChange> ChangedColumns { get; init; } = [];
    public bool HasChanges => AddedColumns.Count > 0 || ChangedColumns.Count > 0;
}
