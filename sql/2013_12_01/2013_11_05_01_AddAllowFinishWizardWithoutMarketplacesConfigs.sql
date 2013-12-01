IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AllowFinishOnlineWizardWithoutMarketplaces')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AllowFinishOnlineWizardWithoutMarketplaces', 0, 'When enabled online wizard can be completed without adding marketplaces')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AllowFinishOfflineWizardWithoutMarketplaces')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AllowFinishOfflineWizardWithoutMarketplaces', 0, 'When enabled offline wizard can be completed without adding marketplaces')
END

GO
