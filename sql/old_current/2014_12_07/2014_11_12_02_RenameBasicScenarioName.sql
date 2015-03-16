IF EXISTS (SELECT 1 FROM PricingModelScenarios WHERE ScenarioName = 'Basic')
BEGIN
	IF NOT EXISTS (SELECT 1 FROM PricingModelScenarios WHERE ScenarioName = 'Basic New')
		UPDATE PricingModelScenarios SET ScenarioName = 'Basic New' WHERE ScenarioName = 'Basic'
END
GO
