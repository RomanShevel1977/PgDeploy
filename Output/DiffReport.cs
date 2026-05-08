using PgDeploy.Diff;

namespace PgDeploy.Output;

public sealed class DiffReport
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public int TablesAdded { get; init; }
    public int TablesChanged { get; init; }
    public int ColumnsAdded { get; init; }
    public int ColumnsChanged { get; init; }

    public static DiffReport From(SchemaDiff diff)
    {
        return new DiffReport
        {
            TablesAdded = diff.AddedTables.Count,
            TablesChanged = diff.ChangedTables.Count,
            ColumnsAdded = diff.ChangedTables.Sum(x => x.AddedColumns.Count),
            ColumnsChanged = diff.ChangedTables.Sum(x => x.ChangedColumns.Count)
        };
    }
}
