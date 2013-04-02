CREATE OR REPLACE PROCEDURE SE_ExecStateStackDepth
(
   pApplicationId in varchar2,
   pStackDepth out Number
)
AS
BEGIN
	SELECT COUNT(*) into pStackDepth FROM StrategyEngine_ExecutionState
   WHERE ApplicationId = pApplicationId;
END;
/
