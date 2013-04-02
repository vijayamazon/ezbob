CREATE OR REPLACE FUNCTION StrategySchedule_StrItemCount
(pStrategyId NUMBER)
 return NUMBER
AS
  l_count number;
BEGIN

  select count(*) into l_count
  from Strategy_Schedule
  where StrategyId = pStrategyId;

  return l_count;

END;
/