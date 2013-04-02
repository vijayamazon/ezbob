create or replace function GetLinksForAppOnUserInput
(
   pApplicationId NUMBER
  ,pEntityType VARCHAR2

)
return sys_refcursor
as
  l_cur sys_refcursor;
begin
 open l_cur for
  SELECT
    StrategyEngine_ExecutionState.CurrentNodePostfix AS NodeName
   FROM  StrategyEngine_ExecutionState INNER JOIN
      Application_Application ON StrategyEngine_ExecutionState.ApplicationId = Application_Application.ApplicationId
       INNER JOIN EntityLink ON Application_Application.StrategyId = EntityLink.EntityId
   WHERE (EntityLink.IsDeleted = 0 OR EntityLink.IsDeleted is null) 
     AND (EntityLink.IsApproved = 1)
     AND (EntityType = pEntityType)
     AND (StrategyEngine_ExecutionState.ApplicationId = pApplicationId);

  
  return l_cur;
end;
/