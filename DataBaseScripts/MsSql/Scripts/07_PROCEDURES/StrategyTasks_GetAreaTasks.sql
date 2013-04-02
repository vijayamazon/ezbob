IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTasks_GetAreaTasks]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTasks_GetAreaTasks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTasks_GetAreaTasks]
(
  @pAreaId int
)
AS
BEGIN

  SELECT * 
  FROM StrategyTasks
  WHERE AreaId = @pAreaId;

END
GO
