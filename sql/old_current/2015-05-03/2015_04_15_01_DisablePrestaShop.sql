
--disable prestashop
UPDATE dbo.MP_MarketplaceType
SET ActiveWizardOnline = 0
	, ActiveDashboardOnline = 0
	, ActiveWizardOffline = 0
	, ActiveDashboardOffline = 0
WHERE InternalId='AE0BC89A-9884-4025-9D96-2755A6CD10EE'
GO

