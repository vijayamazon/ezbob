IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_RemoveStrategy]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_RemoveStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_RemoveStrategy] 
	(@pTaskedStrategyId int)
AS
BEGIN
	DELETE FROM TaskedStrategyParams where TSId = @pTaskedStrategyId;
  DELETE FROM TaskedStrategies where Id = @pTaskedStrategyId
END
GO
