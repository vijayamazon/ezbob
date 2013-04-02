CREATE OR REPLACE PROCEDURE SuspendApplication
 (
   pApplicationId  IN NUMBER,
   pExecutionState IN CLOB,
   pPostfix        IN VARCHAR2,
   pTarget         IN VARCHAR2,
   pLabel          IN VARCHAR2,
   pExecutionPathCurrentItemId  IN NUMBER,
   pExecutionType  IN NUMBER,
   pMessage        IN BLOB
 )
AS
    l_appSpecific NUMBER;
    l_strategyId  NUMBER;
    l_currentNodeId NUMBER;
BEGIN

  DeleteSignalByAppId(pApplicationId => pApplicationId, pAppSpecific => l_appSpecific, pStrategyId => l_strategyId);
  DeleteExecutionStateByAppId(pApplicationId => pApplicationId, pCurrentNodeId => l_currentNodeId);

  insert into Application_Suspended
    (ApplicationId,
     ExecutionState,
     Postfix,
     Target,
     Label,
     ExecutionPathCurrentItemId,
     ExecutionType,
     Message,
     AppSpecific,
     "Date")
  values
    (pApplicationId,
     pExecutionState,
     pPostfix,
     pTarget,
     pLabel,
     pExecutionPathCurrentItemId,
     pExecutionType,
     pMessage,
     l_appSpecific,
     CURRENT_DATE);
END SuspendApplication;
/