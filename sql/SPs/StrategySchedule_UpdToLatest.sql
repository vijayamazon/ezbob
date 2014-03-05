IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategySchedule_UpdToLatest]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategySchedule_UpdToLatest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategySchedule_UpdToLatest] 
	(@pItemId [int])
AS
BEGIN
	DECLARE @pDisplayName [nvarchar](2048);
  DECLARE @pLatestId [int];

  select @pDisplayName = strat.DisplayName
  from Strategy_Schedule sched
  inner join strategy_strategy strat
          on strat.StrategyId = sched.StrategyId
  where sched.ID = @pItemId;

  select @pLatestId = strategy_strategy.StrategyId 
  from strategy_strategy
       where strategy_strategy.DisplayName = @pDisplayName
         and strategy_strategy.TermDate is null
         and strategy_strategy.isdeleted = 0;

  update Strategy_Schedule
  set StrategyId = @pLatestId
  where ID = @pItemID
END
GO
