IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MP_MarketplaceGroup') AND name = 'ActiveWizardOnline')
BEGIN
	ALTER TABLE MP_MarketplaceGroup ADD ActiveWizardOnline BIT NOT NULL CONSTRAINT DF_MarketplaceGroup_ActiveWizOn DEFAULT(1)
	ALTER TABLE MP_MarketplaceGroup ADD ActiveWizardOffline BIT NOT NULL CONSTRAINT DF_MarketplaceGroup_ActiveWizOff DEFAULT(1)
	ALTER TABLE MP_MarketplaceGroup ADD ActiveDashboardOnline BIT NOT NULL CONSTRAINT DF_MarketplaceGroup_ActiveDashOn DEFAULT(1)
	ALTER TABLE MP_MarketplaceGroup ADD ActiveDashboardOffline BIT NOT NULL CONSTRAINT DF_MarketplaceGroup_ActiveDashOff DEFAULT(1)
	ALTER TABLE MP_MarketplaceGroup ADD PriorityOnline INT NOT NULL CONSTRAINT DF_MarketplaceGroup_PrioOn DEFAULT(0)
	ALTER TABLE MP_MarketplaceGroup ADD PriorityOffline INT NOT NULL CONSTRAINT DF_MarketplaceGroup_PrioOff DEFAULT(0)
	ALTER TABLE MP_MarketplaceGroup ADD TimestampCounter ROWVERSION
	
	ALTER TABLE MP_MarketplaceGroup ADD DisplayName NVARCHAR(255) NOT NULL CONSTRAINT DF_MarketplaceGroup_DisplayName DEFAULT('')	
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MP_MarketplaceGroup') AND name = 'ActiveWizardOnline')
BEGIN
	UPDATE MP_MarketplaceGroup SET DisplayName = 'Accounting software' WHERE Name = 'Accounting'
	UPDATE MP_MarketplaceGroup SET DisplayName = 'Online marketplaces' WHERE Name = 'Shop'
	UPDATE MP_MarketplaceGroup SET DisplayName = 'Banks' WHERE Name = 'Bank'
	UPDATE MP_MarketplaceGroup SET DisplayName = 'Other' WHERE Name = 'Other'
END
GO
