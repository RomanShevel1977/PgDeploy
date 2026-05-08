namespace PgDeploy.Core;

public sealed class DiffOptions
{
    public required string SourceConnectionString { get; init; }
    public required string TargetConnectionString { get; init; }
    public string OutputDirectory { get; init; } = "./migrations";
    public string Schema { get; init; } = "public";
    public bool DryRun { get; init; }
    public bool SingleFile { get; init; }
    public string ReportFileName { get; init; } = "pgdeploy-report.json";
}
