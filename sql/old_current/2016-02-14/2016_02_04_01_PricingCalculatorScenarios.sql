SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('PricingCalculatorScenarios') IS NULL
BEGIN
	CREATE TABLE PricingCalculatorScenarios (
		ScenarioID BIGINT IDENTITY(1, 1) NOT NULL,
		ScenarioName NVARCHAR(50) NOT NULL,
		OriginID INT NOT NULL,
		BrokerSetupFee DECIMAL(18, 6) NOT NULL,
		Cogs DECIMAL(18, 6) NOT NULL,
		CollectionRate DECIMAL(18, 6) NOT NULL,
		COSMECollectionRate DECIMAL(18, 6) NOT NULL,
		CostOfDebtPA DECIMAL(18, 6) NOT NULL,
		DebtPercentOfCapital DECIMAL(18, 6) NOT NULL,
		DefaultRateCompanyShare DECIMAL(18, 6) NOT NULL,
		EuCollectionRate DECIMAL(18, 6) NOT NULL,
		InterestOnlyPeriod DECIMAL(18, 6) NOT NULL,
		OpexAndCapex DECIMAL(18, 6) NOT NULL,
		ProfitMarkupPercentsOfRevenue DECIMAL(18, 6) NOT NULL,
		SetupFee DECIMAL(18, 6) NOT NULL,
		TenurePercents DECIMAL(18, 6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_PricingCalculatorScenarios PRIMARY KEY (ScenarioID),
		CONSTRAINT UC_PricingCalculatorScenarios UNIQUE (ScenarioName, OriginID),
		CONSTRAINT CHK_PricingCalculatorScenarios CHECK(LTRIM(RTRIM(ScenarioName)) != ''),
		CONSTRAINT FK_PricingCalculatorScenarios_Origin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin (CustomerOriginID)
	)

	PRINT 'created'
END
GO

CREATE TABLE #res (val INT)
GO

IF NOT EXISTS (SELECT * FROM PricingCalculatorScenarios)
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION

		;WITH ids AS (
			SELECT DISTINCT ScenarioId FROM PricingModelScenarios
		), pcs AS (
			SELECT
				(SELECT TOP 1 ScenarioName FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId) AS ScenarioName,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'TenurePercents') AS TenurePercents,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'SetupFee') AS SetupFee,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'ProfitMarkupPercentsOfRevenue') AS ProfitMarkupPercentsOfRevenue,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'OpexAndCapex') AS OpexAndCapex,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'InterestOnlyPeriod') AS InterestOnlyPeriod,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'EuCollectionRate') AS EuCollectionRate,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'COSMECollectionRate') AS COSMECollectionRate,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'DefaultRateCompanyShare') AS DefaultRateCompanyShare,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'DebtPercentOfCapital') AS DebtPercentOfCapital,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'CostOfDebtPA') AS CostOfDebtPA,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'CollectionRate') AS CollectionRate,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'Cogs') AS Cogs,
				(SELECT ConfigValue FROM PricingModelScenarios WHERE ScenarioId = ids.ScenarioId AND ConfigName = 'BrokerSetupFee') AS BrokerSetupFee
			FROM ids
		) INSERT INTO PricingCalculatorScenarios(
			ScenarioName, OriginID,
			BrokerSetupFee, Cogs, CollectionRate,
			COSMECollectionRate, CostOfDebtPA, DebtPercentOfCapital,
			DefaultRateCompanyShare, EuCollectionRate, InterestOnlyPeriod,
			OpexAndCapex, ProfitMarkupPercentsOfRevenue, SetupFee,
			TenurePercents
		)
		SELECT
			p.ScenarioName,
			OriginID = o.CustomerOriginID,
			p.BrokerSetupFee,
			p.Cogs,
			p.CollectionRate,
			p.COSMECollectionRate,
			p.CostOfDebtPA,
			p.DebtPercentOfCapital,
			p.DefaultRateCompanyShare,
			p.EuCollectionRate,
			p.InterestOnlyPeriod,
			p.OpexAndCapex,
			p.ProfitMarkupPercentsOfRevenue,
			p.SetupFee,
			p.TenurePercents
		FROM
			pcs p
			INNER JOIN CustomerOrigin o ON 1 = 1

		PRINT 'inserted'

		COMMIT TRANSACTION

		INSERT INTO #res (val) VALUES (1)
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION
		PRINT 'rollback'
	END CATCH
END
GO

IF EXISTS (SELECT * FROM #res WHERE val = 1)
BEGIN
	DROP TABLE PricingModelScenarios
	PRINT 'dropped'
END
GO

DROP TABLE #res
GO
