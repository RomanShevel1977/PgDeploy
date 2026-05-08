# PgDeploy

**Safe PostgreSQL schema synchronization and deployment**

Compare two PostgreSQL databases and generate clean, human-readable migration SQL.

## Quick Start

```bash
pgdeploy diff \
  --source "Host=localhost;Database=dev;Username=postgres;Password=postgres" \
  --target "Host=localhost;Database=staging;Username=postgres;Password=postgres" \
  --out ./migrations
```

Apply generated migration:

```bash
pgdeploy apply \
  --conn "Host=localhost;Database=staging;Username=postgres;Password=postgres" \
  --file ./migrations/001_create_users.sql
```

## Features

- Compare PostgreSQL schemas
- Detect missing tables
- Detect missing columns
- Detect column type changes
- Detect nullable/default changes
- Generate migration SQL files
- Dry-run preview
- Apply migration SQL
- JSON diff report

## Dry Run

```bash
pgdeploy diff --source "..." --target "..." --dry-run
```

## Current MVP Scope

Supported: tables, columns, primary keys for new tables, data types, nullability, default values.

Not yet supported: indexes, foreign keys, views, functions, triggers, RLS policies, rollback generation.

## License

MIT
