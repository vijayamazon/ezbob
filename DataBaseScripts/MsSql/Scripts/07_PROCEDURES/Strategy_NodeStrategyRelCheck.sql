IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRelCheck]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_NodeStrategyRelCheck]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[Strategy_NodeStrategyRelCheck]
	@pNodeId int
as
begin
	SELECT DISTINCT [strat].[DisplayName],
	strat.TermDate
    FROM  [Strategy_Strategy] strat 
	  	  INNER JOIN [Strategy_NodeStrategyRel] strRel
		  ON [strat].[StrategyId] = [strRel].[StrategyId]
	WHERE [strRel].[NodeId] = @pNodeId 
		  AND [strat].[IsDeleted] = 0	
end
GO
