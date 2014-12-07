IF NOT EXISTS (SELECT 1 FROM PricingModelScenarios WHERE ScenarioName = 'Basic Repeating')
BEGIN
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'TenurePercents', 0.7)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'SetupFee', 0.03)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'ProfitMarkupPercentsOfRevenue', 0.25)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'OpexAndCapex', 150)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'InterestOnlyPeriod', 0)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'EuCollectionRate', 0.75)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'DefaultRateCompanyShare', 0.7)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'DebtPercentOfCapital', 0.7)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'CostOfDebtPA', 0.16)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'CollectionRate', 0.19)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'Cogs', 200)
	INSERT INTO PricingModelScenarios (ScenarioName, ConfigName, ConfigValue) VALUES ('Basic Repeating', 'BrokerSetupFee', 0)
END
GO

IF EXISTS (SELECT 1 FROM PricingModelScenarios WHERE ScenarioName = 'Basic Repeating' AND ScenarioId IS NULL)
BEGIN
	UPDATE PricingModelScenarios SET ScenarioId = 6 WHERE ScenarioName = 'Basic Repeating' AND ScenarioId IS NULL
END
GO

