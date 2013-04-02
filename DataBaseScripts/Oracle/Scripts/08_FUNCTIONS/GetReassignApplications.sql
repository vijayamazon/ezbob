create or replace function GetReassignApplications (iUserId in number)return sys_refcursor
is
 l_cur sys_refcursor;
begin
 open l_cur for
    select a.ApplicationId,
           a.appcounter,
           a.creationdate,
           (select sn.displayname from strategy_node sn where sn.nodeid = idn.currentnodeid) CurrentNodeName,
           (select sn.nodeid from strategy_node sn where sn.nodeid = idn.currentnodeid) CurrentNodeId,
           (select ss.displayname from strategy_strategy ss where ss.strategyid = a.strategyid) as strategyname,
           (select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = a.strategyid) as creditproduct_name,
           Version,
           nvl(a.childcount, 0) childcount
    from application_Application a,
         StrategyEngine_ExecutionState s,
         Strategy_Strategy t,
         (SELECT max(id) as id, applicationId
              FROM StrategyEngine_ExecutionState
          GROUP BY applicationId) ids
        ,(SELECT max(id) as id, applicationId, currentnodeid
              FROM StrategyEngine_ExecutionState
          GROUP BY applicationId, currentnodeid) idn
     where ids.id = s.id
           and idn.id = s.id
           and ids.applicationId = a.applicationId
           and a.creatoruserid = iUserId
           and (a.LockedByUserId is null or a.LockedByUserId = iUserId)
           and t.StrategyId = a.StrategyId
           and a.parentappid is null
    order by a.appcounter;

return l_cur;
end GetReassignApplications;
/