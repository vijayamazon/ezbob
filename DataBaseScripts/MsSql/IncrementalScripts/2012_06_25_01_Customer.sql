go
ALTER TABLE [Customer] ADD [OverallTurnOver] [decimal](18, 0) NULL
ALTER TABLE [Customer] ADD [WebSiteTurnOver] [decimal](18, 0) NULL
go
update [Customer] set [OverallTurnOver] = ISNULL([LimitedOverallTurnOver],[NonLimitedOverallTurnOver])
update [Customer] set [WebSiteTurnOver] = ISNULL([LimitedWebSiteTurnOver],[NonLimitedWebSiteTurnOver])
go
ALTER TABLE [Customer] DROP COLUMN [LimitedWebSiteTurnOver]
ALTER TABLE [Customer] DROP COLUMN [LimitedOverallTurnOver]
ALTER TABLE [Customer] DROP COLUMN [NonLimitedOverallTurnOver]
ALTER TABLE [Customer] DROP COLUMN [NonLimitedWebSiteTurnOver]