IF EXISTS (SELECT * FROM syscolumns WHERE name = 'MaxScore' AND id = OBJECT_ID('CustomerAnalyticsPersonal'))
	ALTER TABLE CustomerAnalyticsPersonal DROP COLUMN MaxScore
GO

IF EXISTS (SELECT * FROM syscolumns WHERE name = 'MinScore' AND id = OBJECT_ID('CustomerAnalyticsPersonal'))
	ALTER TABLE CustomerAnalyticsPersonal DROP COLUMN MinScore
GO

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('CustomerAnalyticsDirector') IS NULL
	CREATE TABLE CustomerAnalyticsDirector (
		CustomerAnalyticsDirectorID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		AnalyticsDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		MinScore INT NOT NULL,
		MaxScore INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerAnalyticsDirector PRIMARY KEY (CustomerAnalyticsDirectorID),
		CONSTRAINT FK_CustomerAnalyticsDirector_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id)
	)
GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_CustomerAnalyticsDirector')
	CREATE NONCLUSTERED INDEX IDX_CustomerAnalyticsDirector ON CustomerAnalyticsDirector(CustomerID) WHERE IsActive = 1
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CustomerAnalyticsPersonalID' AND xtype = 127 AND id = OBJECT_ID('CustomerAnalyticsPersonal'))
BEGIN
	ALTER TABLE CustomerAnalyticsPersonal DROP CONSTRAINT PK_CustomerAnalyticsPersonal
	ALTER TABLE CustomerAnalyticsPersonal DROP COLUMN CustomerAnalyticsPersonalID
	ALTER TABLE CustomerAnalyticsPersonal ADD CustomerAnalyticsPersonalID BIGINT IDENTITY(1, 1) NOT NULL
	ALTER TABLE CustomerAnalyticsPersonal ADD CONSTRAINT PK_CustomerAnalyticsPersonal PRIMARY KEY (CustomerAnalyticsPersonalID)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'TimestampCounter' AND id = OBJECT_ID('CustomerAnalyticsPersonal'))
	ALTER TABLE CustomerAnalyticsPersonal ADD TimestampCounter ROWVERSION
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CustomerAnalyticsCompanyID' AND xtype = 127 AND id = OBJECT_ID('CustomerAnalyticsCompany'))
BEGIN
	ALTER TABLE CustomerAnalyticsCompany DROP CONSTRAINT PK_CustomerAnalyticsCompany
	ALTER TABLE CustomerAnalyticsCompany DROP COLUMN CustomerAnalyticsCompanyID
	ALTER TABLE CustomerAnalyticsCompany ADD CustomerAnalyticsCompanyID BIGINT IDENTITY(1, 1) NOT NULL
	ALTER TABLE CustomerAnalyticsCompany ADD CONSTRAINT PK_CustomerAnalyticsCompany PRIMARY KEY (CustomerAnalyticsCompanyID)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'TimestampCounter' AND id = OBJECT_ID('CustomerAnalyticsCompany'))
	ALTER TABLE CustomerAnalyticsCompany ADD TimestampCounter ROWVERSION
GO
