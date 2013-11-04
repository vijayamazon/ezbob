IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ActiveWizardOnline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType 
	ADD ActiveWizardOnline BIT
END 
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ActiveDashboardOnline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType 
	ADD ActiveDashboardOnline BIT
END 
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ActiveWizardOffline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType 
	ADD ActiveWizardOffline BIT
END 
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ActiveDashboardOffline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType 
	ADD ActiveDashboardOffline BIT
END 
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'PriorityOnline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType 
	ADD PriorityOnline INT 
END 
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'PriorityOffline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType 
	ADD PriorityOffline INT
END 
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'GroupId' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN
	ALTER TABLE MP_MarketplaceType 
	ADD GroupId INT
	
	ALTER TABLE MP_MarketplaceType
	ADD CONSTRAINT FK_MP_MarketplaceType_MP_MarketplaceGroup FOREIGN KEY (GroupId)
	REFERENCES MP_MarketplaceGroup(Id)
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Ribbon' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType 
	ADD Ribbon NVARCHAR(50)
END 
ELSE
BEGIN
	ALTER TABLE MP_MarketplaceType 
	ALTER COLUMN Ribbon NVARCHAR(50)
END
GO


UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 0
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'A7120CB7-4C93-459B-9901-0E95E7281B59' --ebay
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 1
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B' --amazon
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 2
	, PriorityOffline = 100
	, GroupId = 1
WHERE InternalId = '3FA5E327-FCFD-483B-BA5A-DC1815747A28' --paypal
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 3
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = '57ABA690-EDBA-4D95-89CF-13A34B40E2F1' --ekm
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 0
	, ActiveDashboardOnline = 0
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 5
	, PriorityOffline = 100
	, GroupId = 1
WHERE InternalId = 'FC8F2710-AEDA-481D-86FF-539DD1FB76E0' --paypoint
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 4
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'AFCA0E18-05E3-400F-8AF4-B1BCAE09375C' --volusion
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 6
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'A5E96D38-FD2E-4E54-9E0C-276493C950A6' --play
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 1
	, ActiveDashboardOffline = 1
	, PriorityOnline = 7
	, PriorityOffline = 1
	, GroupId = 3
WHERE InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF' --yodlee
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 1
	, ActiveDashboardOffline = 1
	, PriorityOnline = 8
	, PriorityOffline = 3
	, GroupId = 1
WHERE InternalId = '737691E8-5C77-48EF-B01B-7348E24094B6' --freeagent
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 11
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'A386F349-8E41-4BA9-B709-90332466D42D' -- shopify
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 1
	, ActiveDashboardOffline = 1
	, PriorityOnline = 10
	, PriorityOffline = 2
	, GroupId = 1
WHERE InternalId = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C' --xero
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 1
	, ActiveDashboardOffline = 1
	, PriorityOnline = 12
	, PriorityOffline = 4
	, GroupId = 1
WHERE InternalId = '4966BB57-0146-4E3D-AA24-F092D90B7923' --sage
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 1
	, ActiveDashboardOffline = 1
	, PriorityOnline = 13
	, PriorityOffline = 5
	, GroupId = 1
WHERE InternalId = 'A755B4F6-D4EC-4D80-96A2-B2849BD800AC' --kashflow
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 1
	, ActiveDashboardOffline = 1
	, PriorityOnline = 14
	, PriorityOffline = 0
	, GroupId = 1
WHERE InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA' --hmrc
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 15
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'A660B9CC-8BB1-4A37-9597-507622AEBF9E' --magento
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 16
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'AE0BC89A-9884-4025-9D96-2755A6CD10EE' --prestashop
GO

UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 1
	, ActiveDashboardOnline = 1
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
	, PriorityOnline = 17
	, PriorityOffline = 100
	, GroupId = 2
WHERE InternalId = 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996' --bigcommerce
GO


