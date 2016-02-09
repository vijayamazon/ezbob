IF OBJECT_ID('GetPricingScenarioDetails') IS NULL
	EXECUTE('CREATE PROCEDURE GetPricingScenarioDetails AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetPricingScenarioDetails
@ScenarioID BIGINT
AS
BEGIN
	SELECT
		p.TenurePercents,
		p.SetupFee,
		p.ProfitMarkupPercentsOfRevenue,
		p.OpexAndCapex,
		p.InterestOnlyPeriod,
		p.EuCollectionRate,
		p.COSMECollectionRate,
		p.DefaultRateCompanyShare,
		p.DebtPercentOfCapital,
		p.CostOfDebtPA,
		p.CollectionRate,
		p.Cogs,
		p.BrokerSetupFee,
		p.OriginID
	FROM
		PricingCalculatorScenarios p
	WHERE
		p.ScenarioID = @ScenarioID
END
GO
