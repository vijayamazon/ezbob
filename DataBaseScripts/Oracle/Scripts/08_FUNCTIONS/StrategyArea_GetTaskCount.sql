CREATE OR REPLACE FUNCTION StrategyArea_GetTaskCount
(
pAreaID NUMBER
)
RETURN NUMBER
as
  l_count NUMBER;
begin
  select count(*) into l_count
  from StrategyTasks
  where AreaID = pAreaId;

return l_count;

end;
/