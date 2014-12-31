IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavePricingModelConfigsForScenario]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SavePricingModelConfigsForScenario]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SavePricingModelConfigsForScenario]
	@ScenarioName VARCHAR(50),
	@TenurePercents DECIMAL(18,6),
	@SetupFee DECIMAL(18,6),
	@ProfitMarkupPercentsOfRevenue DECIMAL(18,6),
	@OpexAndCapex DECIMAL(18,6),
	@InterestOnlyPeriod DECIMAL(18,6),
	@EuCollectionRate DECIMAL(18,6),
	@COSMECollectionRate DECIMAL(18,6),
	@DefaultRateCompanyShare DECIMAL(18,6),
	@DebtPercentOfCapital DECIMAL(18,6),
	@CostOfDebtPA DECIMAL(18,6),
	@CollectionRate DECIMAL(18,6),
	@Cogs DECIMAL(18,6),
	@BrokerSetupFee DECIMAL(18,6)
AS
BEGIN
	UPDATE PricingModelScenarios SET ConfigValue = @TenurePercents WHERE ScenarioName = @ScenarioName AND ConfigName='TenurePercents'
	UPDATE PricingModelScenarios SET ConfigValue = @SetupFee WHERE ScenarioName = @ScenarioName AND ConfigName='SetupFee'
	UPDATE PricingModelScenarios SET ConfigValue = @ProfitMarkupPercentsOfRevenue WHERE ScenarioName = @ScenarioName AND ConfigName='ProfitMarkupPercentsOfRevenue'
	UPDATE PricingModelScenarios SET ConfigValue = @OpexAndCapex WHERE ScenarioName = @ScenarioName AND ConfigName='OpexAndCapex'
	UPDATE PricingModelScenarios SET ConfigValue = @InterestOnlyPeriod WHERE ScenarioName = @ScenarioName AND ConfigName='InterestOnlyPeriod'
	UPDATE PricingModelScenarios SET ConfigValue = @EuCollectionRate WHERE ScenarioName = @ScenarioName AND ConfigName='EuCollectionRate'
	UPDATE PricingModelScenarios SET ConfigValue = @COSMECollectionRate WHERE ScenarioName = @ScenarioName AND ConfigName='COSMECollectionRate'
	UPDATE PricingModelScenarios SET ConfigValue = @DefaultRateCompanyShare WHERE ScenarioName = @ScenarioName AND ConfigName='DefaultRateCompanyShare'
	UPDATE PricingModelScenarios SET ConfigValue = @DebtPercentOfCapital WHERE ScenarioName = @ScenarioName AND ConfigName='DebtPercentOfCapital'
	UPDATE PricingModelScenarios SET ConfigValue = @CostOfDebtPA WHERE ScenarioName = @ScenarioName AND ConfigName='CostOfDebtPA'
	UPDATE PricingModelScenarios SET ConfigValue = @Cogs WHERE ScenarioName = @ScenarioName AND ConfigName='Cogs'
	UPDATE PricingModelScenarios SET ConfigValue = @CollectionRate WHERE ScenarioName = @ScenarioName AND ConfigName='CollectionRate'
	UPDATE PricingModelScenarios SET ConfigValue = @BrokerSetupFee WHERE ScenarioName = @ScenarioName AND ConfigName='BrokerSetupFee'
END
GO
