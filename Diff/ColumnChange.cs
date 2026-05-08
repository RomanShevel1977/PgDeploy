using PgDeploy.Schema;

namespace PgDeploy.Diff;

public sealed class ColumnChange
{
    public required ColumnDefinition SourceColumn { get; init; }
    public required ColumnDefinition TargetColumn { get; init; }
    public List<ColumnChangeKind> Changes { get; init; } = [];
}

public enum ColumnChangeKind
{
    TypeChanged,
    NullabilityChanged,
    DefaultChanged
}
