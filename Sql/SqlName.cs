namespace PgDeploy.Sql;

public static class SqlName
{
    public static string Quote(string name)
    {
        return "\"" + name.Replace("\"", "\"\"") + "\"";
    }

    public static string Qualified(string schema, string table)
    {
        return $"{Quote(schema)}.{Quote(table)}";
    }
}
