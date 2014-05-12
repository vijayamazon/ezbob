IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelTenurePercents')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelTenurePercents', '0.5', 'Pricing model tenure in percentages (0-1)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelDefaultRateCompanyShare')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelDefaultRateCompanyShare', '0.7', 'Pricing model default rate company share in percentages (0-1)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelInterestOnlyPeriod')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelInterestOnlyPeriod', '0', 'Pricing model interest only period in months')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelCollectionRate')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelCollectionRate', '0.19', 'Pricing model collection rate in percentages (0-1)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelEuCollectionRate')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelEuCollectionRate', '0.75', 'Pricing model EU collection rate in percentages (0-1)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelCogs')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelCogs', '1000', 'Pricing model cogs in pounds')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelDebtOutOfTotalCapital')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelDebtOutOfTotalCapital', '0.6', 'Pricing model debt out of total capital in percentages (0-1)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelCostOfDebtPA')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelCostOfDebtPA', '0.16', 'Pricing model cost of debt per anual year in percentages (0-1)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelOpexAndCapex')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelOpexAndCapex', '150', 'Pricing model opex and capex in pounds')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelProfitMarkupPercentsOfRevenue')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelProfitMarkupPercentsOfRevenue', '0.25', 'Pricing model profit markup percents of revenue in percentages (0-1)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PricingModelSetupFee')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PricingModelSetupFee', '0.015', 'Pricing model setup fee in percentages (0-1)')
END
GO
