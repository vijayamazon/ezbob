UPDATE 
	ConfigurationVariables 
SET 
	Value = 'True' 
WHERE 
	Name = 'IsSmsValidationActive'
GO



UPDATE 
	MP_MarketplaceType
SET 
	ActiveWizardOnline = 1, 
	ActiveDashboardOnline = 1, 
	ActiveWizardOffline = 1, 
	ActiveDashboardOffline = 1
WHERE 
	Name = 'CompanyFiles'
GO
