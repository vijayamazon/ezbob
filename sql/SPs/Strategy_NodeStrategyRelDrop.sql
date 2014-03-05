IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRelDrop]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_NodeStrategyRelDrop]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_NodeStrategyRelDrop] 
	(@pStrategyId int)
AS
BEGIN
	DELETE FROM Strategy_NodeStrategyRel
			WHERE StrategyId = @pStrategyId
END
GO
