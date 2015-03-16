IF NOT EXISTS (SELECT * FROM MP_MarketplaceGroup WHERE Name = 'Common')
	INSERT INTO MP_MarketplaceGroup (Name, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline, DisplayName)
		VALUES ('Common', 1, 1, 1, 1, 1, 1, 'Common')
GO

IF NOT EXISTS (SELECT * FROM MP_MarketplaceGroup WHERE Name = 'Webstores')
	INSERT INTO MP_MarketplaceGroup (Name, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline, DisplayName)
		VALUES ('Webstores', 1, 1, 1, 1, 2, 2, 'Webstores')
GO

IF NOT EXISTS (SELECT * FROM MP_MarketplaceGroup WHERE Name = 'Accounting')
	INSERT INTO MP_MarketplaceGroup (Name, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline, DisplayName)
		VALUES ('Accounting', 1, 1, 1, 1, 3, 3, 'Accounting software')
GO

UPDATE MP_MarketplaceGroup SET
	ActiveWizardOnline = 0,
	ActiveWizardOffline = 0,
	ActiveDashboardOnline = 0,
	ActiveDashboardOffline = 0,
	PriorityOnline = 0,
	PriorityOffline = 0
GO

UPDATE MP_MarketplaceGroup SET
	ActiveWizardOnline = 1,
	ActiveWizardOffline = 1,
	ActiveDashboardOnline = 1,
	ActiveDashboardOffline = 1,
	PriorityOnline = 1,
	PriorityOffline = 1,
	DisplayName = 'Common'
WHERE
	Name = 'Common'
GO

UPDATE MP_MarketplaceGroup SET
	ActiveWizardOnline = 1,
	ActiveWizardOffline = 1,
	ActiveDashboardOnline = 1,
	ActiveDashboardOffline = 1,
	PriorityOnline = 2,
	PriorityOffline = 2,
	DisplayName = 'Webstores'
WHERE
	Name = 'Webstores'
GO

UPDATE MP_MarketplaceGroup SET
	ActiveWizardOnline = 1,
	ActiveWizardOffline = 1,
	ActiveDashboardOnline = 1,
	ActiveDashboardOffline = 1,
	PriorityOnline = 3,
	PriorityOffline = 3,
	DisplayName = 'Accounting software'
WHERE
	Name = 'Accounting'
GO

SELECT
	t.Name AS MpName,
	t.InternalId AS MpId,
	g.Name AS GroupName,
	t.ActiveWizardOnline,
	t.ActiveWizardOffline,
	t.ActiveDashboardOnline,
	t.ActiveDashboardOffline,
	t.PriorityOnline,
	t.PriorityOffline
INTO
	#t
FROM
	MP_MarketplaceType t,
	MP_MarketplaceGroup g
WHERE
	t.Id = g.Id AND t.Id != g.Id

INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('eBay', 'Common', 'A7120CB7-4C93-459B-9901-0E95E7281B59', 1, 1, 1, 1, 0, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Amazon', 'Common', 'A4920125-411F-4BB9-A52D-27E8A00D0A3B', 1, 1, 1, 1, 1, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Pay Pal', 'Common', '3FA5E327-FCFD-483B-BA5A-DC1815747A28', 1, 1, 1, 1, 2, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('EKM', 'Webstores', '57ABA690-EDBA-4D95-89CF-13A34B40E2F1', 1, 1, 1, 1, 3, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Volusion', 'Webstores', 'AFCA0E18-05E3-400F-8AF4-B1BCAE09375C', 1, 1, 1, 1, 4, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('PayPoint', 'Webstores', 'FC8F2710-AEDA-481D-86FF-539DD1FB76E0', 0, 0, 0, 0, 5, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Play', 'Webstores', 'A5E96D38-FD2E-4E54-9E0C-276493C950A6', 1, 1, 1, 1, 6, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Yodlee', 'Common', '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF', 1, 1, 1, 1, 7, 1)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('FreeAgent', 'Accounting', '737691E8-5C77-48EF-B01B-7348E24094B6', 1, 1, 1, 1, 8, 3)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Shopify', 'Webstores', 'A386F349-8E41-4BA9-B709-90332466D42D', 1, 1, 1, 1, 11, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Xero', 'Accounting', 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C', 1, 1, 1, 1, 10, 2)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Sage', 'Accounting', '4966BB57-0146-4E3D-AA24-F092D90B7923', 1, 1, 1, 1, 12, 4)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('KashFlow', 'Accounting', 'A755B4F6-D4EC-4D80-96A2-B2849BD800AC', 1, 1, 1, 1, 13, 5)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('HMRC', 'Common', 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA', 1, 1, 1, 1, 14, 0)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Magento', 'Webstores', 'A660B9CC-8BB1-4A37-9597-507622AEBF9E', 1, 1, 1, 1, 15, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Prestashop', 'Webstores', 'AE0BC89A-9884-4025-9D96-2755A6CD10EE', 1, 1, 1, 1, 16, 100)
INSERT INTO #t (MpName, GroupName, MpId, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Bigcommerce', 'Webstores', 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996', 1, 1, 1, 1, 17, 100)

UPDATE MP_MarketplaceType SET
	GroupId = g.Id,
	ActiveWizardOnline = #t.ActiveWizardOnline,
	ActiveWizardOffline = #t.ActiveWizardOffline,
	ActiveDashboardOnline = #t.ActiveDashboardOnline,
	ActiveDashboardOffline = #t.ActiveDashboardOffline,
	PriorityOnline = #t.PriorityOnline,
	PriorityOffline = #t.PriorityOffline
FROM
	MP_MarketplaceType t, #t, MP_MarketplaceGroup g
WHERE
	t.InternalId = #t.MpId AND #t.GroupName = g.Name

DROP TABLE #t
GO
