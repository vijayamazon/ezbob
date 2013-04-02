create or replace function GetApplicationState return sys_refcursor
as
 l_cur sys_refcursor;
begin
 open l_cur for
select 
*
from
(
    select a.ApplicationId,
           a.appcounter,
           a.creationdate,
           nvl((select sn.displayname from strategy_node sn where sn.nodeid = idn.currentnodeid), 'SE') CurrentNodeName,
           (select sn.nodeid from strategy_node sn where sn.nodeid = idn.currentnodeid) CurrentNodeId,
           (select ss.displayname from strategy_strategy ss where ss.strategyid = a.strategyid) as strategyname,
           (select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = a.strategyid and ROWNUM=1) as creditproduct_name,
           Version,
           nvl(a.childcount, 0) childcount,
           (select su.userid from security_user su where su.userid = a.creatoruserid) as userId,
           (select su.username from security_user su where su.userid = a.creatoruserid) as userName,
           (select su.fullname from security_user su where su.userid = a.creatoruserid) as userFullName
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
           and t.StrategyId = a.StrategyId
           and a.parentappid is null
union all
select
      a.ApplicationId
     ,a.appcounter
     ,a.creationdate
     ,nvl((SELECT sn.Name FROM Application_History ah,Strategy_Node sn
              WHERE (ah.ApplicationId = a.applicationid) and ah.AppHistoryId = (SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID), 'SE') as CurrentNodeName
     ,(SELECT ah.CurrentNodeID FROM Application_History ah,Strategy_Node sn
              WHERE (ah.ApplicationId = a.applicationid) and ah.AppHistoryId = (SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID) as CurrentNodeId
     ,(select ss.displayname from strategy_strategy ss where ss.strategyid = a.strategyid) as strategyname
     ,(select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = a.strategyid and ROWNUM=1) as creditproduct_name
     ,Version
     ,nvl(a.childcount, 0) as childcount
     ,(select su.userid from security_user su where su.userid = a.creatoruserid) as userId
     ,(select su.username from security_user su where su.userid = a.creatoruserid) as userName
     ,(select su.fullname from security_user su where su.userid = a.creatoruserid) as userFullName
    from
        application_application a
       ,Application_Suspended susp
    where
        (a.state = 4 or a.state = 5 or a.state = 6 or a.state = 7)
        and a.applicationid = susp.applicationid
        and a.parentappid is null
) allapp           
order by allapp.appcounter;

 return l_cur;
end GetApplicationState;
/
