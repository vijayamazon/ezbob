IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_GetStrategyCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_GetStrategyCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_GetStrategyCount]
(
  @pTaskId int
)
AS
BEGIN

  DECLARE @cnt int;

  SELECT @cnt = COUNT(*) 
    FROM TaskedStrategies
    WHERE TaskId = @pTaskId;

  SELECT @cnt;
  RETURN @cnt;

END
GO
