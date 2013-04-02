IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategySchedule_StrItemCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategySchedule_StrItemCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategySchedule_StrItemCount]
(
  @pStrategyId int
)
AS
BEGIN
  
  DECLARE @cnt int;

  SELECT @cnt = COUNT(*) 
    FROM Strategy_Schedule
    WHERE StrategyId = @pStrategyId;

  SELECT @cnt;
  RETURN @cnt;

END
GO
