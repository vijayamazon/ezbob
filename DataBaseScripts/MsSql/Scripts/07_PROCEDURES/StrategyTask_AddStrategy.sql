IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_AddStrategy]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_AddStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_AddStrategy]
(
  @pTaskId int,
  @pLabel nvarchar(64),
  @pStrategyId int
)
AS
BEGIN

  DECLARE @id int;

  INSERT INTO TaskedStrategies
  ([Label], [StrategyId], [TaskId])
  VALUES
  (@pLabel, @pStrategyId, @pTaskId);

  set @id = @@IDENTITY;

  SELECT @id;
  RETURN @id;

END
GO
