SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_PricingScenario') IS NULL
	EXECUTE('CREATE PROCEDURE AV_PricingScenario AS SELECT 1')
GO

ALTER PROCEDURE AV_PricingScenario
@Amount INT, @HasLoans BIT
AS
BEGIN

DECLARE @SmallLoan INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name='SmallLoanScenarioLimit')


DECLARE @ScenarioName NVARCHAR(30)

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

DECLARE @TenurePercents DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='TenurePercents')
DECLARE @ProfitMarkupPercentsOfRevenue DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='ProfitMarkupPercentsOfRevenue')
DECLARE @OpexAndCapex DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='OpexAndCapex')
DECLARE @InterestOnlyPeriod DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='InterestOnlyPeriod')
DECLARE @EuCollectionRate DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='EuCollectionRate')
DECLARE @DefaultRateCompanyShare DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='DefaultRateCompanyShare')
DECLARE @DebtPercentOfCapital DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='DebtPercentOfCapital')
DECLARE @CostOfDebtPA DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='CostOfDebtPA')
DECLARE @CollectionRate DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='CollectionRate')
DECLARE @Cogs DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='Cogs')
DECLARE @BrokerSetupFee DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='BrokerSetupFee')
DECLARE @CosmeCollectionRate DECIMAL(18,6) = (SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioName=@ScenarioName AND ConfigName='CosmeCollectionRate')


SELECT
	@ScenarioName AS ScenarioName,
	@TenurePercents AS TenurePercents,
	@ProfitMarkupPercentsOfRevenue AS ProfitMarkupPercentsOfRevenue,
	@OpexAndCapex AS OpexAndCapex,
	@InterestOnlyPeriod AS InterestOnlyPeriod,
	@EuCollectionRate AS EuCollectionRate,
	@DefaultRateCompanyShare AS DefaultRateCompanyShare,
	@DebtPercentOfCapital AS DebtPercentOfCapital,
	@CostOfDebtPA AS CostOfDebtPA,
	@CollectionRate AS CollectionRate,
	@Cogs AS Cogs,
	@BrokerSetupFee AS BrokerSetupFee,
	@CosmeCollectionRate AS CosmeCollectionRate
END

GO
