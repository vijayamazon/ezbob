CREATE OR REPLACE FUNCTION StrategyTask_UpdateStrategy
(
pTaskId NUMBER,
pLabel VARCHAR2,
pStrategyId NUMBER,
pId NUMBER
)
RETURN NUMBER
as
begin

  UPDATE TaskedStrategies
  SET
   Label = pLabel,
   StrategyId = pStrategyId,
   TaskId = pTaskId
  WHERE id=pId;

  DELETE FROM TaskedStrategyParams WHERE TSId=pId;

return pId;

end;
/