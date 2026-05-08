namespace PgDeploy.Schema;

public sealed class ColumnDefinition
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public required string FormattedType { get; init; }
    public required bool IsNullable { get; init; }
    public string? DefaultValue { get; init; }
    public int OrdinalPosition { get; init; }
}
