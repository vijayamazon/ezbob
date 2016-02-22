IF OBJECT_ID('SavePricingModelConfigsForScenario') IS NULL
	EXECUTE('CREATE PROCEDURE SavePricingModelConfigsForScenario AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SavePricingModelConfigsForScenario
@ScenarioID BIGINT,
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
	UPDATE PricingCalculatorScenarios SET
		TenurePercents = @TenurePercents,
		SetupFee = @SetupFee,
		ProfitMarkupPercentsOfRevenue = @ProfitMarkupPercentsOfRevenue,
		OpexAndCapex = @OpexAndCapex,
		InterestOnlyPeriod = @InterestOnlyPeriod,
		EuCollectionRate = @EuCollectionRate,
		COSMECollectionRate = @COSMECollectionRate,
		DefaultRateCompanyShare = @DefaultRateCompanyShare,
		DebtPercentOfCapital = @DebtPercentOfCapital,
		CostOfDebtPA = @CostOfDebtPA,
		Cogs = @Cogs,
		CollectionRate = @CollectionRate,
		BrokerSetupFee = @BrokerSetupFee
	WHERE
		ScenarioID = @ScenarioID
END
GO
