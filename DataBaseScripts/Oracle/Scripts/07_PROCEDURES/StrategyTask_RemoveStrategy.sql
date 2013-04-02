CREATE OR REPLACE PROCEDURE StrategyTask_RemoveStrategy
(
pTaskedStrategyId NUMBER
)
as
begin

  DELETE FROM TaskedStrategyParams where TSID = pTaskedStrategyId;
  DELETE FROM TaskedStrategies where ID = pTaskedStrategyId;

end;
/