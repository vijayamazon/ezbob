CREATE OR REPLACE PROCEDURE StrategyEngine_FinishStrategy

(pApplicationId number, pErrorMsg varchar2, pUserId number)

 AS

  pstate number;

BEGIN
  --SET NOCOUNT ON;

  if pErrorMsg is null then
    pstate := 2; -- 2-strategy has been finished without errors;
  else
    pstate := 3; -- 3-strategy has been finished with errors.
  end if;

  update Application_Application a
     set state            = pstate,
         ErrorMsg         = pErrorMsg,
         LockedByUserId   = null,
         LastUpdateDate   = sysdate, --GetDate(),
         IsTimeLimitExceeded = ( select( case
                                           when a.creationdate + NVL( s.executionduration, 1E10)/86400 < SYSDATE then 1
                                           else 0
                                         end )
                                   from strategy_strategy s
                                  where a.strategyid = s.strategyid )
   where ApplicationId = pApplicationID
     and LockedByUserId = pUserId;

  if SQL%ROWCOUNT = 0
  --@@ROWCOUNT = 0
   then
    App_RaiseAppNotExistError(pApplicationId);
  end if;

   DELETE FROM StrategyEngine_ExecutionState
   WHERE ApplicationID = pApplicationID;

   DELETE FROM APPLICATION_NODESETTING
   WHERE ApplicationID = pApplicationID;
END;
/