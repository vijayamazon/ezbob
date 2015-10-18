SET QUOTED_IDENTIFIER ON
GO

DELETE FROM MP_MarketplaceGroup WHERE Name IN ('Shop', 'Other')
GO

UPDATE MP_MarketplaceGroup SET
	ActiveWizardOnline = 1,
	ActiveWizardOffline = 1,
	ActiveDashboardOnline = 1,
	ActiveDashboardOffline = 1
GO
