IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategySchedule_GetNextRun]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategySchedule_GetNextRun]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategySchedule_GetNextRun]
(
  @pStrategyId int
)
AS
BEGIN

  DECLARE @cnt int,
          @dte datetime;

  SELECT @cnt = COUNT(*)
    FROM Strategy_Schedule
    WHERE StrategyId = @pStrategyId;

  IF @cnt > 0
    begin
      SELECT @dte = MIN(NextRun)
      FROM Strategy_Schedule
      WHERE StrategyId = @pStrategyId;
    end
  else
    begin
      SET @dte = NULL;
    end;

  SELECT @dte;

END
GO
