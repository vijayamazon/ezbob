IF OBJECT_ID('GetPricingModelScenarios') IS NULL
	EXECUTE('CREATE PROCEDURE GetPricingModelScenarios AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetPricingModelScenarios
AS
BEGIN
	SELECT DISTINCT
		p.ScenarioName,
		p.OriginID,
		Origin = o.Name
	FROM
		PricingCalculatorScenarios p
		INNER JOIN CustomerOrigin o ON p.OriginID = o.CustomerOriginID
	ORDER BY
		o.Name,
		p.ScenarioName
END
GO
