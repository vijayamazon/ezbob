IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRelCheck]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_NodeStrategyRelCheck]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_NodeStrategyRelCheck] 
	(@pNodeId int)
AS
BEGIN
	SELECT DISTINCT [strat].[DisplayName],
	strat.TermDate
    FROM  [Strategy_Strategy] strat 
	  	  INNER JOIN [Strategy_NodeStrategyRel] strRel
		  ON [strat].[StrategyId] = [strRel].[StrategyId]
	WHERE [strRel].[NodeId] = @pNodeId 
		  AND [strat].[IsDeleted] = 0
END
GO
