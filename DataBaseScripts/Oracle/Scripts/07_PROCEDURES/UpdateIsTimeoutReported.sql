CREATE OR REPLACE procedure UpdateIsTimeoutReported
  (
    pExecutionStateId in Number
  )
as
begin
   UPDATE STRATEGYENGINE_EXECUTIONSTATE
      SET ISTIMEOUTREPORTED = 1
    WHERE STRATEGYENGINE_EXECUTIONSTATE.ID = pExecutionStateId;
end;
/