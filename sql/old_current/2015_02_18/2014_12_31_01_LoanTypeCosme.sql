ALTER TABLE LoanSource DROP CONSTRAINT CHK_LoanSource_MaxInterest
ALTER TABLE LoanSource ALTER COLUMN MaxInterest DECIMAL(18,6)
ALTER TABLE LoanSource  ADD CONSTRAINT CHK_LoanSource_MaxInterest CHECK ([MaxInterest] IS NULL OR [MaxInterest]>(0))
GO

IF NOT EXISTS (SELECT * FROM LoanSource WHERE LoanSourceName = 'COSME')
BEGIN 
INSERT INTO dbo.LoanSource
	(
	  LoanSourceID
	, LoanSourceName
	, MaxInterest
	, DefaultRepaymentPeriod
	, IsCustomerRepaymentPeriodSelectionAllowed
	, MaxEmployeeCount
	, MaxAnnualTurnover
	, IsDefault
	, AlertOnCustomerReasonType
	)
VALUES
	(
	3
	, 'COSME'
	, 0.0225
	, 12
	, 0
	, NULL
	, NULL
	, 0
	, NULL
	)

END
GO

IF NOT EXISTS (SELECT * FROM PricingModelScenarios WHERE ConfigName = 'COSMECollectionRate')
BEGIN
INSERT INTO dbo.PricingModelScenarios
	(
	ScenarioId
	, ScenarioName
	, ConfigName
	, ConfigValue
	)
VALUES
	(
	1
	, 'Basic New'
	, 'COSMECollectionRate'
	, 0.5
	)


INSERT INTO dbo.PricingModelScenarios
	(
	ScenarioId
	, ScenarioName
	, ConfigName
	, ConfigValue
	)
VALUES
	(
	2
	, 'Broker'
	, 'COSMECollectionRate'
	, 0.5
	)


INSERT INTO dbo.PricingModelScenarios
	(
	ScenarioId
	, ScenarioName
	, ConfigName
	, ConfigValue
	)
VALUES
	(
	3
	, 'Small Loan'
	, 'COSMECollectionRate'
	, 0.5
	)


INSERT INTO dbo.PricingModelScenarios
	(
	ScenarioId
	, ScenarioName
	, ConfigName
	, ConfigValue
	)
VALUES
	(
	4
	, 'Non-ltd'
	, 'COSMECollectionRate'
	, 0.5
	)


INSERT INTO dbo.PricingModelScenarios
	(
	ScenarioId
	, ScenarioName
	, ConfigName
	, ConfigValue
	)
VALUES
	(
	5
	, 'Sole Traders'
	, 'COSMECollectionRate'
	, 0.5
	)


INSERT INTO dbo.PricingModelScenarios
	(
	ScenarioId
	, ScenarioName
	, ConfigName
	, ConfigValue
	)
VALUES
	(
	6
	, 'Basic Repeating'
	, 'COSMECollectionRate'
	, 0.5
	)

END 
GO