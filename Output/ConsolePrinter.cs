using PgDeploy.Diff;

namespace PgDeploy.Output;

public static class ConsolePrinter
{
    public static void PrintDiff(SchemaDiff diff)
    {
        Console.WriteLine();
        if (!diff.HasChanges)
        {
            Console.WriteLine("No schema changes found.");
            return;
        }

        Console.WriteLine("Schema changes:");
        foreach (var table in diff.AddedTables)
            Console.WriteLine($"[+] table {table.FullName}");

        foreach (var tableChange in diff.ChangedTables)
        {
            foreach (var column in tableChange.AddedColumns)
                Console.WriteLine($"[+] column {tableChange.SourceTable.FullName}.{column.Name}");

            foreach (var columnChange in tableChange.ChangedColumns)
            {
                var kinds = string.Join(", ", columnChange.Changes);
                Console.WriteLine($"[~] column {tableChange.SourceTable.FullName}.{columnChange.SourceColumn.Name} ({kinds})");
            }
        }
    }
}
