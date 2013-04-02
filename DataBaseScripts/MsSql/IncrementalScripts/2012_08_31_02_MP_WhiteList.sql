DROP TABLE [dbo].[MP_WhiteList]
GO

CREATE TABLE dbo.MP_WhiteList
	(
	Id int NOT NULL IDENTITY (1, 1),
	Name nvarchar(500) NOT NULL,
	MarketPlaceTypeGuid uniqueidentifier NOT NULL
	)  ON [PRIMARY]
GO
DECLARE @v sql_variant 
SET @v = N'List of marketplaces, that are not checked for uniqueness'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'dbo', N'TABLE', N'MP_WhiteList', NULL, NULL
GO
ALTER TABLE dbo.MP_WhiteList ADD CONSTRAINT
	PK_MP_WhiteList PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.MP_WhiteList SET (LOCK_ESCALATION = TABLE)
GO
