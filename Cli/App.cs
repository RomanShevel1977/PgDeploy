using System.CommandLine;
using PgDeploy.Commands;

namespace PgDeploy.Cli;

public static class App
{
    public static async Task<int> RunAsync(string[] args)
    {
        var root = new RootCommand("PgDeploy - Safe PostgreSQL schema synchronization and deployment");
        root.AddCommand(DiffCommand.Create());
        root.AddCommand(ApplyCommand.Create());
        return await root.InvokeAsync(args);
    }
}
