using Npgsql;

namespace PgDeploy.Schema;

public sealed class PostgresSchemaReader
{
    private readonly string _connectionString;

    public PostgresSchemaReader(string connectionString) => _connectionString = connectionString;

    public async Task<DatabaseSchema> ReadAsync(string schema, CancellationToken ct = default)
    {
        var db = new DatabaseSchema { SchemaName = schema };
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        db.Tables.AddRange(await ReadTablesAsync(conn, schema, ct));
        await ReadPrimaryKeysAsync(conn, db, ct);
        return db;
    }

    private static async Task<List<TableDefinition>> ReadTablesAsync(NpgsqlConnection conn, string schema, CancellationToken ct)
    {
        const string sql = """
        SELECT c.table_schema, c.table_name, c.column_name, c.ordinal_position,
               c.is_nullable, c.column_default, c.data_type,
               pg_catalog.format_type(a.atttypid, a.atttypmod) AS formatted_type
        FROM information_schema.columns c
        JOIN pg_catalog.pg_namespace n ON n.nspname = c.table_schema
        JOIN pg_catalog.pg_class cls ON cls.relname = c.table_name AND cls.relnamespace = n.oid
        JOIN pg_catalog.pg_attribute a ON a.attrelid = cls.oid AND a.attname = c.column_name AND a.attnum > 0 AND NOT a.attisdropped
        JOIN information_schema.tables t ON t.table_schema = c.table_schema AND t.table_name = c.table_name
        WHERE c.table_schema = @schema AND t.table_type = 'BASE TABLE'
        ORDER BY c.table_name, c.ordinal_position;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("schema", schema);
        var tables = new Dictionary<string, TableDefinition>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var tableSchema = reader.GetString(0);
            var tableName = reader.GetString(1);
            var columnName = reader.GetString(2);
            var ordinal = reader.GetInt32(3);
            var isNullable = reader.GetString(4).Equals("YES", StringComparison.OrdinalIgnoreCase);
            var defaultValue = reader.IsDBNull(5) ? null : reader.GetString(5);
            var dataType = reader.GetString(6);
            var formattedType = reader.GetString(7);

            if (!tables.TryGetValue(tableName, out var table))
            {
                table = new TableDefinition { Schema = tableSchema, Name = tableName };
                tables.Add(tableName, table);
            }

            table.Columns.Add(new ColumnDefinition
            {
                Name = columnName,
                DataType = dataType,
                FormattedType = formattedType,
                IsNullable = isNullable,
                DefaultValue = defaultValue,
                OrdinalPosition = ordinal
            });
        }

        return tables.Values.OrderBy(x => x.Name).ToList();
    }

    private static async Task ReadPrimaryKeysAsync(NpgsqlConnection conn, DatabaseSchema db, CancellationToken ct)
    {
        const string sql = """
        SELECT tc.table_name, tc.constraint_name, kcu.column_name, kcu.ordinal_position
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
          ON kcu.constraint_schema = tc.constraint_schema
         AND kcu.constraint_name = tc.constraint_name
         AND kcu.table_schema = tc.table_schema
         AND kcu.table_name = tc.table_name
        WHERE tc.table_schema = @schema AND tc.constraint_type = 'PRIMARY KEY'
        ORDER BY tc.table_name, kcu.ordinal_position;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("schema", db.SchemaName);
        var pkMap = new Dictionary<string, (string Name, List<string> Columns)>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var tableName = reader.GetString(0);
            var constraintName = reader.GetString(1);
            var columnName = reader.GetString(2);
            if (!pkMap.TryGetValue(tableName, out var pk))
                pkMap[tableName] = pk = (constraintName, []);
            pk.Columns.Add(columnName);
        }

        foreach (var (tableName, pk) in pkMap)
        {
            var table = db.FindTable(tableName);
            if (table is null) continue;
            table.PrimaryKey = new PrimaryKeyDefinition { Name = pk.Name, Columns = pk.Columns };
        }
    }
}
