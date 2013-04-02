CREATE OR REPLACE PROCEDURE RestoreApplicationFromSuspend
  (
    pApplicationId IN NUMBER
   )
AS
 l_id Number;
 l_appSpecific Number;
BEGIN
  Select SEQ_SE_EXECUTIONSTATE.NEXTVAL into l_id from dual;
    
  DELETE FROM StrategyEngine_ExecutionState WHERE ApplicationId = pApplicationId;
  INSERT INTO StrategyEngine_ExecutionState
    (Id,
     ApplicationId,
     CurrentNodeId,
     CurrentNodePostfix,
     Data,
     StartTime,
     IsTimeoutReported)
  SELECT
    l_id,
    ApplicationId,
    null,
    Postfix,
    ExecutionState,
    CURRENT_DATE,
    null
  FROM Application_Suspended
  WHERE ApplicationId = pApplicationId
    AND NOT(Postfix is null OR ExecutionState is null);

  Select SEQ_Signal.NEXTVAL into l_id from dual;

  SELECT Version
  INTO l_appSpecific
  FROM Application_Application
  WHERE ApplicationId = pApplicationId;

  DELETE FROM Signal WHERE ApplicationId = pApplicationId;
  INSERT INTO Signal
    (Id,
     Target,
     Label,
     Status,
     StartTime,
     AppSpecific,
     ApplicationId,
     ExecutionType,
     Message)
  SELECT
    l_id,
    Target,
    Label,
    0,
    CURRENT_DATE,
    l_appSpecific,
    ApplicationId,
    ExecutionType,
    Message
  FROM Application_Suspended
  WHERE ApplicationId = pApplicationId;

  DELETE
  FROM Application_Suspended
  WHERE ApplicationId = pApplicationId;

  UPDATE Application_Application
  SET State = 0
  WHERE ApplicationId = pApplicationId;

END RestoreApplicationFromSuspend;
/