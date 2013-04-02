create or replace function ExternalNodesToBeResetList
(
  pEntityType in varchar2
)
return sys_refcursor
as
  l_cur sys_refcursor;
begin
 open l_cur for
  SELECT
    STRATEGYENGINE_EXECUTIONSTATE.ID,
    STRATEGYENGINE_EXECUTIONSTATE.CURRENTNODEID NODEID,
    STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID,
    APPLICATION_APPLICATION.CREATORUSERID,
    APPLICATION_APPLICATION.VERSION
   FROM
    STRATEGYENGINE_EXECUTIONSTATE
    INNER JOIN APPLICATION_APPLICATION
        ON STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID = APPLICATION_APPLICATION.APPLICATIONID
    INNER JOIN EntityLink
        ON APPLICATION_APPLICATION.StrategyId = EntityLink.EntityId 
   WHERE  EntityLink.EntityType = pEntityType
     AND (EntityLink.IsDeleted = 0 OR EntityLink.IsDeleted is null) 
     AND (EntityLink.IsApproved = 1);
  
  return l_cur;
end;
/