SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_PricingScenario') IS NULL
	EXECUTE('CREATE PROCEDURE AV_PricingScenario AS SELECT 1')
GO

ALTER PROCEDURE AV_PricingScenario
@Amount INT,
@HasLoans BIT,
@CustomerID INT
AS
BEGIN
	DECLARE @SmallLoan INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'SmallLoanScenarioLimit')

	DECLARE @ScenarioName NVARCHAR(50)

	IF @Amount <= @SmallLoan
	BEGIN
		SET @ScenarioName = 'Small Loan'		
	END
	ELSE
	BEGIN
		IF @HasLoans = 1
		BEGIN
			SET @ScenarioName = 'Basic Repeating'
		END
		ELSE
		BEGIN
			SET @ScenarioName = 'Basic New'
		END
	END

	SELECT
		p.ScenarioName,
		p.TenurePercents,
		p.ProfitMarkupPercentsOfRevenue,
		p.OpexAndCapex,
		p.InterestOnlyPeriod,
		p.EuCollectionRate,
		p.DefaultRateCompanyShare,
		p.DebtPercentOfCapital,
		p.CostOfDebtPA,
		p.CollectionRate,
		p.Cogs,
		p.BrokerSetupFee,
		p.CosmeCollectionRate
	FROM
		PricingCalculatorScenarios p
		INNER JOIN Customer c
			ON p.OriginID = c.OriginID
			AND c.Id = @CustomerID	
	WHERE
		p.ScenarioName = @ScenarioName
END
GO
