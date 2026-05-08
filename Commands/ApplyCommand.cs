using System.CommandLine;
using Npgsql;

namespace PgDeploy.Commands;

public static class ApplyCommand
{
    public static Command Create()
    {
        var connOption = new Option<string>("--conn", "Target PostgreSQL connection string") { IsRequired = true };
        var fileOption = new Option<string>("--file", "SQL migration file to apply") { IsRequired = true };
        var transactionOption = new Option<bool>("--transaction", () => true, "Run migration inside a transaction");

        var command = new Command("apply", "Apply a generated migration SQL file")
        {
            connOption, fileOption, transactionOption
        };

        command.SetHandler(async (string conn, string file, bool useTransaction) =>
        {
            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"Migration file not found: {file}");
                Environment.ExitCode = 1;
                return;
            }

            var sql = await File.ReadAllTextAsync(file);
            await using var connection = new NpgsqlConnection(conn);
            await connection.OpenAsync();

            if (useTransaction)
            {
                await using var tx = await connection.BeginTransactionAsync();
                try
                {
                    await using var cmd = new NpgsqlCommand(sql, connection, tx);
                    await cmd.ExecuteNonQueryAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            else
            {
                await using var cmd = new NpgsqlCommand(sql, connection);
                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine($"Applied migration: {file}");
        }, connOption, fileOption, transactionOption);

        return command;
    }
}
