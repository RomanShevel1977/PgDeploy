using PgDeploy.Diff;

namespace PgDeploy.Sql;

public sealed class MigrationFileWriter
{
    private readonly MigrationSqlGenerator _generator;
    public MigrationFileWriter(MigrationSqlGenerator generator) => _generator = generator;

    public async Task<List<string>> WriteAsync(SchemaDiff diff, string outputDirectory, bool singleFile)
    {
        var files = new List<string>();
        if (!diff.HasChanges) return files;

        if (singleFile)
        {
            var file = Path.Combine(outputDirectory, $"{Timestamp()}_migration.sql");
            await File.WriteAllTextAsync(file, _generator.GenerateFullMigration(diff));
            files.Add(file);
            return files;
        }

        var index = 1;
        foreach (var table in diff.AddedTables)
        {
            var file = Path.Combine(outputDirectory, $"{index:000}_create_{SafeFileName(table.Name)}.sql");
            await File.WriteAllTextAsync(file, _generator.GenerateCreateTable(table));
            files.Add(file);
            index++;
        }

        foreach (var tableChange in diff.ChangedTables)
        {
            foreach (var column in tableChange.AddedColumns)
            {
                var file = Path.Combine(outputDirectory, $"{index:000}_add_{SafeFileName(tableChange.SourceTable.Name)}_{SafeFileName(column.Name)}.sql");
                await File.WriteAllTextAsync(file, _generator.GenerateAddColumn(tableChange.SourceTable, column));
                files.Add(file);
                index++;
            }

            foreach (var columnChange in tableChange.ChangedColumns)
            {
                var file = Path.Combine(outputDirectory, $"{index:000}_alter_{SafeFileName(tableChange.SourceTable.Name)}_{SafeFileName(columnChange.SourceColumn.Name)}.sql");
                await File.WriteAllTextAsync(file, _generator.GenerateAlterColumn(tableChange.SourceTable, columnChange));
                files.Add(file);
                index++;
            }
        }

        return files;
    }

    private static string Timestamp() => DateTime.UtcNow.ToString("yyyyMMddHHmmss");

    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).ToLowerInvariant();
    }
}
