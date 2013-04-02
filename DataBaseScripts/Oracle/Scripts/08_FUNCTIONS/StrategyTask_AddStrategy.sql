CREATE OR REPLACE FUNCTION StrategyTask_AddStrategy
(
pTaskId NUMBER,
pLabel VARCHAR2,
pStrategyId NUMBER
)
RETURN NUMBER
as
  l_id NUMBER;
begin
  select SEQ_TaskedStrategy.nextval into l_id from dual;

  INSERT INTO TaskedStrategies
  (ID, Label, StrategyId, TaskId)
  VALUES
  (l_id, pLabel, pStrategyId, pTaskId);

return l_id;

end;
/