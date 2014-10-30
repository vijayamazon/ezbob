ALTER TABLE SiteAnalytics ALTER COLUMN Source NVARCHAR(MAX) NULL
GO

IF OBJECT_ID('UC_SiteAnalyticsCodes') IS NULL
	ALTER TABLE SiteAnalyticsCodes ADD CONSTRAINT UC_SiteAnalyticsCodes UNIQUE (Name)
GO
