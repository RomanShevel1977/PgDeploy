namespace PgDeploy.Schema;

public sealed class PrimaryKeyDefinition
{
    public required string Name { get; init; }
    public required List<string> Columns { get; init; }
}
