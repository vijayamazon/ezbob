UPDATE 
	MP_MarketplaceType
SET 
	ActiveWizardOffline = 1
WHERE 
	Name = 'Yodlee'
GO

UPDATE 
	ConfigurationVariables
SET 
	Value = '0'
WHERE 
	Name = 'AllowFinishOfflineWizardWithoutMarketplaces'
GO

