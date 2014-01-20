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

SELECT
	t.Name AS MpName,
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

INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('eBay', 'Common', 1, 1, 1, 1, 0, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Amazon', 'Common', 1, 1, 1, 1, 1, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Pay Pal', 'Common', 1, 1, 1, 1, 2, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('EKM', 'Webstores', 1, 1, 1, 1, 3, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Volusion', 'Webstores', 1, 1, 1, 1, 4, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('PayPoint', 'Webstores', 0, 0, 0, 0, 5, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Play', 'Webstores', 1, 1, 1, 1, 6, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Yodlee', 'Common', 1, 1, 1, 1, 7, 1)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('FreeAgent', 'Accounting', 1, 1, 1, 1, 8, 3)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Shopify', 'Webstores', 1, 1, 1, 1, 11, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Xero', 'Accounting', 1, 1, 1, 1, 10, 2)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Sage', 'Accounting', 1, 1, 1, 1, 12, 4)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('KashFlow', 'Accounting', 1, 1, 1, 1, 13, 5)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('HMRC', 'Common', 1, 1, 1, 1, 14, 0)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Magento', 'Webstores', 1, 1, 1, 1, 15, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Prestashop', 'Webstores', 1, 1, 1, 1, 16, 100)
INSERT INTO #t (MpName, GroupName, ActiveWizardOnline, ActiveWizardOffline, ActiveDashboardOnline, ActiveDashboardOffline, PriorityOnline, PriorityOffline) VALUES('Bigcommerce', 'Webstores', 1, 1, 1, 1, 17, 100)

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
	t.Name = #t.MpName AND #t.GroupName = g.Name

DROP TABLE #t
GO
