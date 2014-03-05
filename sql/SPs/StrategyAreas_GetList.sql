IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyAreas_GetList]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyAreas_GetList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyAreas_GetList]
AS
BEGIN
	SELECT * 
  FROM StrategyAreas
END
GO
