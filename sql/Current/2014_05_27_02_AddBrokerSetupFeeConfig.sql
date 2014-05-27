IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelBrokerSetupFee')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelBrokerSetupFee', '0', 'Pricing model broker setup fee in percentages (0-1)')
END
GO
