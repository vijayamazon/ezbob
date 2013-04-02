CREATE OR REPLACE FUNCTION StrategySchedule_GetNextRun
(pStrategyId NUMBER)
 return DATE
AS
  l_count number;
  l_date date;
BEGIN

  select count(*) into l_count
  from Strategy_Schedule
  where StrategyId = pStrategyId;

  if l_count > 0 then
    select MIN(NextRun) into l_date
    from Strategy_Schedule
    where StrategyId = pStrategyId;
  else
    select to_date('31.12.9999', 'DD.MM.YYYY') into l_date from dual;
  end if;

  return l_date;

END;
/