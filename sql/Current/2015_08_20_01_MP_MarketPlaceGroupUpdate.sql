IF NOT EXISTS (SELECT * FROM MP_MarketplaceGroup WHERE Name='VAT')
BEGIN
	INSERT INTO dbo.MP_MarketplaceGroup
		(
		Name
		, ActiveWizardOnline
		, ActiveWizardOffline
		, ActiveDashboardOnline
		, ActiveDashboardOffline
		, PriorityOnline
		, PriorityOffline
		, DisplayName
		)
	VALUES
		(
		'VAT'
		, 1
		, 1
		, 1
		, 1
		, 1
		, 1
		, 'VAT reports'
		)
	
	
	INSERT INTO dbo.MP_MarketplaceGroup
		(
		Name
		, ActiveWizardOnline
		, ActiveWizardOffline
		, ActiveDashboardOnline
		, ActiveDashboardOffline
		, PriorityOnline
		, PriorityOffline
		, DisplayName
		)
	VALUES
		(
		'Bank'
		, 1
		, 1
		, 1
		, 1
		, 2
		, 2
		, 'Bank statements'
		)
	
	INSERT INTO dbo.MP_MarketplaceGroup
		(
		Name
		, ActiveWizardOnline
		, ActiveWizardOffline
		, ActiveDashboardOnline
		, ActiveDashboardOffline
		, PriorityOnline
		, PriorityOffline
		, DisplayName
		)
	VALUES
		(
		'Documents'
		, 1
		, 1
		, 1
		, 1
		, 3
		, 3
		, 'Financial accounts'
		)
	
	UPDATE dbo.MP_MarketplaceGroup
	SET PriorityOnline = 4, PriorityOffline = 4
	WHERE Name = 'Common' 
	
	
	UPDATE dbo.MP_MarketplaceGroup
	SET  PriorityOnline = 5, PriorityOffline = 5
	WHERE Name = 'Webstores'
	
	
	UPDATE dbo.MP_MarketplaceGroup
	SET  PriorityOnline = 6, PriorityOffline = 6
	WHERE Name = 'Accounting'
END 

DECLARE @Vat INT = (SELECT Id FROM dbo.MP_MarketplaceGroup WHERE Name = 'VAT')
DECLARE @Bank INT = (SELECT Id FROM dbo.MP_MarketplaceGroup WHERE Name = 'Bank')
DECLARE @Documents INT = (SELECT Id FROM dbo.MP_MarketplaceGroup WHERE Name = 'Documents')

UPDATE dbo.MP_MarketplaceType
SET GroupId = @Documents
WHERE InternalId = '1C077670-6D6C-4CE9-BEBC-C1F9A9723908' 

UPDATE dbo.MP_MarketplaceType
SET GroupId = @Bank
WHERE InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF' 

UPDATE dbo.MP_MarketplaceType
SET GroupId = @Vat
WHERE InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
GO

	

