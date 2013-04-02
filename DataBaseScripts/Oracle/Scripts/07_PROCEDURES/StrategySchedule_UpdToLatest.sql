CREATE OR REPLACE PROCEDURE StrategySchedule_UpdToLatest
(
  pItemId NUMBER
)

as
  l_DisplayName varchar2(2048);
  l_LatestId number;
begin

  select strat.DisplayName into l_DisplayName
  from Strategy_Schedule sched
  inner join strategy_strategy strat
          on strat.StrategyId = sched.StrategyId
  where sched.ID = pItemId;

  select strategy_strategy.StrategyId into l_LatestId
  from strategy_strategy
       where strategy_strategy.DisplayName = l_DisplayName
         and strategy_strategy.TermDate is null
         and strategy_strategy.isdeleted = 0;

  update Strategy_Schedule
  set StrategyId = l_LatestId
  where ID = pItemID;

end;
/