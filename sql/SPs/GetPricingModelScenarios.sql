IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPricingModelScenarios]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPricingModelScenarios]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPricingModelScenarios]
AS
BEGIN
	SELECT DISTINCT ScenarioName FROM PricingModelScenarios 
END
GO
