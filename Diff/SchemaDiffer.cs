using PgDeploy.Schema;

namespace PgDeploy.Diff;

public sealed class SchemaDiffer
{
    public SchemaDiff Compare(DatabaseSchema source, DatabaseSchema target)
    {
        var diff = new SchemaDiff();
        foreach (var sourceTable in source.Tables.OrderBy(x => x.Name))
        {
            var targetTable = target.FindTable(sourceTable.Name);
            if (targetTable is null)
            {
                diff.AddedTables.Add(sourceTable);
                continue;
            }
            var tableChange = CompareTable(sourceTable, targetTable);
            if (tableChange.HasChanges) diff.ChangedTables.Add(tableChange);
        }
        return diff;
    }

    private static TableChange CompareTable(TableDefinition sourceTable, TableDefinition targetTable)
    {
        var change = new TableChange { SourceTable = sourceTable, TargetTable = targetTable };
        foreach (var sourceColumn in sourceTable.Columns.OrderBy(x => x.OrdinalPosition))
        {
            var targetColumn = targetTable.FindColumn(sourceColumn.Name);
            if (targetColumn is null)
            {
                change.AddedColumns.Add(sourceColumn);
                continue;
            }
            var columnChanges = CompareColumn(sourceColumn, targetColumn);
            if (columnChanges.Count > 0)
                change.ChangedColumns.Add(new ColumnChange { SourceColumn = sourceColumn, TargetColumn = targetColumn, Changes = columnChanges });
        }
        return change;
    }

    private static List<ColumnChangeKind> CompareColumn(ColumnDefinition source, ColumnDefinition target)
    {
        var changes = new List<ColumnChangeKind>();
        if (!string.Equals(source.FormattedType, target.FormattedType, StringComparison.OrdinalIgnoreCase)) changes.Add(ColumnChangeKind.TypeChanged);
        if (source.IsNullable != target.IsNullable) changes.Add(ColumnChangeKind.NullabilityChanged);
        if (!NormalizeDefault(source.DefaultValue).Equals(NormalizeDefault(target.DefaultValue), StringComparison.OrdinalIgnoreCase)) changes.Add(ColumnChangeKind.DefaultChanged);
        return changes;
    }

    private static string NormalizeDefault(string? value) => string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
}
