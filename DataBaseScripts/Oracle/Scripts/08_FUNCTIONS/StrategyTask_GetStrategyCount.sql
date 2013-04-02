CREATE OR REPLACE FUNCTION StrategyTask_GetStrategyCount
(
pTaskID NUMBER
)
RETURN NUMBER
as
  l_count NUMBER;
begin

  select count(*) into l_count
  from TaskedStrategies
  where TaskID = pTaskId;

return l_count;

end;
/