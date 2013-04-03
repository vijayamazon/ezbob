IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_UpdateStrategy]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_UpdateStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_UpdateStrategy]
(
  @pTaskId int,
  @pLabel nvarchar(64),
  @pStrategyId int,
  @pId int
)
AS
BEGIN

  UPDATE TaskedStrategies
  SET
   [Label] = @pLabel,
   [StrategyId] = @pStrategyId,
   [TaskId] = @pTaskId
  WHERE id=@pId;


  DELETE FROM TaskedStrategyParams WHERE TSId=@pId;


  SELECT @pId;
  RETURN @pId;

END
GO
