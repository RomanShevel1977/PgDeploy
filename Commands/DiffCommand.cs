using System.CommandLine;
using PgDeploy.Core;
using PgDeploy.Diff;
using PgDeploy.Output;
using PgDeploy.Schema;
using PgDeploy.Sql;

namespace PgDeploy.Commands;

public static class DiffCommand
{
    public static Command Create()
    {
        var sourceOption = new Option<string>("--source", "Source PostgreSQL connection string") { IsRequired = true };
        var targetOption = new Option<string>("--target", "Target PostgreSQL connection string") { IsRequired = true };
        var outOption = new Option<string>("--out", () => "./migrations", "Output directory for generated migration files");
        var schemaOption = new Option<string>("--schema", () => "public", "PostgreSQL schema to compare");
        var dryRunOption = new Option<bool>("--dry-run", "Preview diff without writing migration files");
        var singleFileOption = new Option<bool>("--single-file", "Generate one migration file instead of multiple files");
        var reportOption = new Option<string>("--report", () => "pgdeploy-report.json", "Report file name");

        var command = new Command("diff", "Compare two PostgreSQL databases and generate migration SQL")
        {
            sourceOption, targetOption, outOption, schemaOption, dryRunOption, singleFileOption, reportOption
        };

        command.SetHandler(async (string source, string target, string output, string schema, bool dryRun, bool singleFile, string reportName) =>
        {
            var options = new DiffOptions
            {
                SourceConnectionString = source,
                TargetConnectionString = target,
                OutputDirectory = output,
                Schema = schema,
                DryRun = dryRun,
                SingleFile = singleFile,
                ReportFileName = reportName
            };

            await ExecuteAsync(options);
        }, sourceOption, targetOption, outOption, schemaOption, dryRunOption, singleFileOption, reportOption);

        return command;
    }

    private static async Task ExecuteAsync(DiffOptions options)
    {
        Console.WriteLine($"Reading source schema: {options.Schema}");
        var sourceSchema = await new PostgresSchemaReader(options.SourceConnectionString).ReadAsync(options.Schema);

        Console.WriteLine($"Reading target schema: {options.Schema}");
        var targetSchema = await new PostgresSchemaReader(options.TargetConnectionString).ReadAsync(options.Schema);

        var diff = new SchemaDiffer().Compare(sourceSchema, targetSchema);
        ConsolePrinter.PrintDiff(diff);

        if (options.DryRun)
        {
            Console.WriteLine();
            Console.WriteLine("Dry run enabled. No files were written.");
            return;
        }

        Directory.CreateDirectory(options.OutputDirectory);

        var writtenFiles = await new MigrationFileWriter(new MigrationSqlGenerator())
            .WriteAsync(diff, options.OutputDirectory, options.SingleFile);

        var reportPath = Path.Combine(options.OutputDirectory, options.ReportFileName);
        await ReportWriter.WriteAsync(DiffReport.From(diff), reportPath);

        Console.WriteLine();
        Console.WriteLine($"Migration files written: {writtenFiles.Count}");
        foreach (var file in writtenFiles)
            Console.WriteLine($"  {file}");
        Console.WriteLine($"Report written: {reportPath}");
    }
}
