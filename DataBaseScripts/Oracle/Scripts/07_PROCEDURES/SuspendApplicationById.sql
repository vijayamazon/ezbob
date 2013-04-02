CREATE OR REPLACE PROCEDURE SuspendApplicationById
(
       pApplicationId IN NUMBER,
       pExecutionPathCurrentItemId IN NUMBER
)
AS
       l_ExecutionState CLOB;
       l_Postfix        VARCHAR2(4000);
       l_Target         VARCHAR2(4000);
       l_Label          VARCHAR2(4000);
       l_Message        BLOB;
       l_appSpecific NUMBER;
       l_strategyId  NUMBER;
       l_currentNodeId NUMBER;
       l_executionType NUMBER;
BEGIN
  BEGIN
     SELECT
       data,
       CurrentNodePostfix
     into
       l_ExecutionState,
       l_Postfix
     FROM
       StrategyEngine_ExecutionState
     WHERE applicationId = pApplicationId;

     SELECT
       s.Target,
       s.Label,
       s.Message,
       s.ExecutionType
     into
       l_Target,
       l_Label,
       l_Message,
       l_executionType
     FROM
       Signal s
     WHERE
       s.applicationId = pApplicationId;
   EXCEPTION
    WHEN no_data_found
    THEN
        return; 
   END;

     DeleteSignalByAppId(pApplicationId, l_appSpecific, l_strategyId);
     DeleteExecutionStateByAppId(pApplicationId => pApplicationId, pCurrentNodeId => l_currentNodeId);
     insert into Application_Suspended
     (
        ApplicationId
      , ExecutionState
      , Postfix
      , Target
      , Label
      , ExecutionPathCurrentItemId
      , Message
      , ExecutionType
      , AppSpecific
      , "Date"
     )
     values
     (
        pApplicationId
      , l_ExecutionState
      , l_Postfix
      , l_Target
      , l_Label
      , pExecutionPathCurrentItemId
      , l_Message
      , l_executionType
      , l_appSpecific
      , sysdate);
END SuspendApplicationById;
/
