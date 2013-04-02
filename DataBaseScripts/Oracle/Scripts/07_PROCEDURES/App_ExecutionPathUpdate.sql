CREATE OR REPLACE procedure App_ExecutionPathUpdate
(
    pApplicationId in Number,
    pExecutionPathBin  IN BLOB
)
as
begin
    UPDATE Application_Application
    SET ExecutionPathBin = pExecutionPathBin, ExecutionPath = null
    WHERE ApplicationId = pApplicationId;

end App_ExecutionPathUpdate;
/
