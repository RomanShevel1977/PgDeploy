using System.Text.Json;

namespace PgDeploy.Output;

public static class ReportWriter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static async Task WriteAsync(DiffReport report, string path)
    {
        var json = JsonSerializer.Serialize(report, Options);
        await File.WriteAllTextAsync(path, json);
    }
}
