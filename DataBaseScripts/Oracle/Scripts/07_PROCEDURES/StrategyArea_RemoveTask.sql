CREATE OR REPLACE PROCEDURE StrategyArea_RemoveTask
(
pTaskId NUMBER
)
as
begin

  DELETE FROM StrategyTasks where ID = pTaskId;

end;
/