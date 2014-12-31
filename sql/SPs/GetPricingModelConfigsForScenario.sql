IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPricingModelConfigsForScenario]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPricingModelConfigsForScenario]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPricingModelConfigsForScenario]
	@ScenarioName VARCHAR(50)
AS
BEGIN
	SELECT 
		ConfigName, 
		ConfigValue
	INTO 
		#GetPricingModelConfigsForScenario 
	FROM 
		PricingModelScenarios
	WHERE
		ScenarioName = @ScenarioName
		
	SELECT
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'TenurePercents') AS TenurePercents,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'SetupFee') AS SetupFee,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'ProfitMarkupPercentsOfRevenue') AS ProfitMarkupPercentsOfRevenue,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'OpexAndCapex') AS OpexAndCapex,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'InterestOnlyPeriod') AS InterestOnlyPeriod,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'EuCollectionRate') AS EuCollectionRate,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'COSMECollectionRate') AS COSMECollectionRate,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'DefaultRateCompanyShare') AS DefaultRateCompanyShare,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'DebtPercentOfCapital') AS DebtPercentOfCapital,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'CostOfDebtPA') AS CostOfDebtPA,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'CollectionRate') AS CollectionRate,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'Cogs') AS Cogs,
		(SELECT ConfigValue FROM #GetPricingModelConfigsForScenario WHERE ConfigName = 'BrokerSetupFee') AS BrokerSetupFee
END
GO
