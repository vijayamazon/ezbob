IF OBJECT_ID('AlibabaCampaigns') IS NULL
BEGIN
	CREATE TABLE AlibabaCampaigns (
		Name NVARCHAR(255) NOT NULL,
		DateFrom DATETIME NOT NULL,
		DateTo DATETIME NOT NULL,
		CONSTRAINT PK_AlibabaCampaigns PRIMARY KEY (Name)
	)
END
GO

IF NOT EXISTS (SELECT * FROM AlibabaCampaigns WHERE Name = '2014-10-29')
	INSERT INTO AlibabaCampaigns (Name, DateFrom, DateTo) VALUES ('2014-10-29', 'Sep 1 2012', 'Nov 5 2014')
GO

IF NOT EXISTS (SELECT * FROM AlibabaCampaigns WHERE Name = '2014-11-05')
	INSERT INTO AlibabaCampaigns (Name, DateFrom, DateTo) VALUES ('2014-11-05', 'Nov 5 2014', 'Nov 5 2020')
GO
