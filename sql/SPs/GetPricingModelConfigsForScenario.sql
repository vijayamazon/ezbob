IF OBJECT_ID('GetPricingModelConfigsForScenario') IS NULL
	EXECUTE('CREATE PROCEDURE GetPricingModelConfigsForScenario AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetPricingModelConfigsForScenario
@ScenarioName VARCHAR(50),
@CustomerID INT
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
		p.BrokerSetupFee
	FROM
		PricingCalculatorScenarios p
		INNER JOIN Customer c ON p.OriginID = c.OriginID AND c.Id = @CustomerID
	WHERE
		p.ScenarioName = @ScenarioName
END
GO
