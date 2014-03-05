IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyArea_RemoveTask]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyArea_RemoveTask]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyArea_RemoveTask] 
	(@pTaskId int)
AS
BEGIN
	DELETE FROM StrategyTasks WHERE Id = @pTaskId
END
GO
