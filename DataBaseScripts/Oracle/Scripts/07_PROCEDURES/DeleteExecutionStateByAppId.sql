CREATE OR REPLACE PROCEDURE DeleteExecutionStateByAppId(pApplicationId IN NUMBER, pCurrentNodeId OUT NUMBER)
AS
BEGIN
 BEGIN
	SELECT se.currentnodeid into pCurrentNodeId FROM StrategyEngine_ExecutionState se WHERE se.applicationid = pApplicationId;
 EXCEPTION
	WHEN OTHERS
	THEN pCurrentNodeId := null;
 END;	
 DELETE FROM StrategyEngine_ExecutionState WHERE applicationId = pApplicationId;
END DeleteExecutionStateByAppId;
/
