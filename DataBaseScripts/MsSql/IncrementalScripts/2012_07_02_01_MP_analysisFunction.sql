IF EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_MP_AnalyisisFunctionName') 
	ALTER TABLE dbo.MP_AnalyisisFunction DROP CONSTRAINT IX_MP_AnalyisisFunctionName
go
create unique index [IX_MP_AnalyisisFunctionName] ON [dbo].[MP_AnalyisisFunction] 
(
[Name],
[MarketPlaceId]
)
go