IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAvailableStrategyTypes]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAvailableStrategyTypes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAvailableStrategyTypes]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT DISTINCT StrategyType FROM Strategy_Strategy
END
GO
