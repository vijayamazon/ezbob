IF NOT EXISTS (SELECT * FROM SiteAnalyticsCodes a WHERE a.Name='UKUsers')
BEGIN
INSERT INTO dbo.SiteAnalyticsCodes (Name, Description) VALUES
	('UKUsers'	, 'Total number of users to your property for the requested time period from UK')


INSERT INTO dbo.SiteAnalyticsCodes (Name, Description) VALUES
	('UKNewUsers', 'The number of users whose session on your property was marked as a first-time session from UK')

INSERT INTO dbo.SiteAnalyticsCodes (Name, Description) VALUES
	('WorldWideUsers', 'Total number of users to your property for the requested time period from World Wide (except Israel and Ukraine)')

INSERT INTO dbo.SiteAnalyticsCodes (Name, Description) VALUES
	('WorldWideNewUsers', 'The number of users whose session on your property was marked as a first-time session from World Wide (except Israel and Ukraine)')

INSERT INTO dbo.SiteAnalyticsCodes (Name, Description) VALUES
	('ReturningUsers', 'WorldWideUsers - WorldWideNewUsers')

INSERT INTO dbo.SiteAnalyticsCodes (Name, Description) VALUES
	('UKReturningUsers', 'UKUsers - UKNewUsers')

DELETE FROM SiteAnalytics
END
