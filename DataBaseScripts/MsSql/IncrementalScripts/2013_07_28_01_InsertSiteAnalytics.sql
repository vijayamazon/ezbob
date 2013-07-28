IF EXISTS (SELECT * FROM SiteAnalyticsCodes WHERE Name = 'ReturningVisitors')
	UPDATE SiteAnalyticsCodes SET Description = 'Site returning visitors from World Wide' WHERE Name = 'ReturningVisitors'
ELSE
	INSERT INTO SiteAnalyticsCodes (Description, Name) VALUES('Site returning visitors from World Wide', 'ReturningVisitors')
GO

IF EXISTS (SELECT * FROM SiteAnalyticsCodes WHERE Name = 'NewVisitors')
	UPDATE SiteAnalyticsCodes SET Description = 'Site new visitors from World Wide' WHERE Name = 'NewVisitors'
ELSE
	INSERT INTO SiteAnalyticsCodes (Description, Name) VALUES('Site new visitors from World Wide', 'NewVisitors')
GO

IF EXISTS (SELECT * FROM SiteAnalyticsCodes WHERE Name = 'PagePacnet')
	UPDATE SiteAnalyticsCodes SET Description = 'Pacnet page unique visitors' WHERE Name = 'PagePacnet'
ELSE
	INSERT INTO SiteAnalyticsCodes (Description, Name) VALUES('Pacnet page unique visitors', 'PagePacnet')
GO

IF EXISTS (SELECT * FROM SiteAnalyticsCodes WHERE Name = 'PageLogon')
	UPDATE SiteAnalyticsCodes SET Description = 'Logon page unique visitors' WHERE Name = 'PageLogon'
ELSE
	INSERT INTO SiteAnalyticsCodes (Description, Name) VALUES('Logon page unique visitors', 'PageLogon')
GO

IF EXISTS (SELECT * FROM SiteAnalyticsCodes WHERE Name = 'PageDashboard')
	UPDATE SiteAnalyticsCodes SET Description = 'Customer dashboard unique visitors' WHERE Name = 'PageDashboard'
ELSE
	INSERT INTO SiteAnalyticsCodes (Description, Name) VALUES('Customer dashboard unique visitors', 'PageDashboard')
GO

IF EXISTS (SELECT * FROM SiteAnalyticsCodes WHERE Name = 'PageGetCash')
	UPDATE SiteAnalyticsCodes SET Description = 'GetCash page unique visitors' WHERE Name = 'PageGetCash'
ELSE
	INSERT INTO SiteAnalyticsCodes (Description, Name) VALUES('GetCash page unique visitors', 'PageGetCash')
GO

IF OBJECT_ID('InsertSiteAnalytics') IS NOT NULL
	DROP PROCEDURE InsertSiteAnalytics
GO

CREATE PROCEDURE InsertSiteAnalytics
@Date DATETIME,
@CodeName NVARCHAR(300),
@Value INT
AS
BEGIN	
	IF NOT EXISTS (SELECT Id FROM SiteAnalyticsCodes WHERE Name = @CodeName)
		INSERT INTO SiteAnalyticsCodes (Name, Description)
			Values (@CodeName, '@' + @CodeName)

	INSERT INTO SiteAnalytics ([Date], SiteAnalyticsCode, SiteAnalyticsValue)
	SELECT
		@Date,
		c.Id,
		@Value
	FROM
		SiteAnalyticsCodes c
	WHERE
		c.Name = @CodeName
END
GO