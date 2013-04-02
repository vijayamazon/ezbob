create or replace function GetHistoryByApplicationId(iAppId in number) return sys_refcursor
is
 l_cur sys_refcursor;
begin
 open l_cur for
    select
        ah.apphistoryid
       ,ah.applicationid
       ,su.userid
       ,su.username
       ,su.fullname
       ,ah.securityapplicationid
       ,(select sa.name from security_application sa where sa.applicationid = ah.securityapplicationid) as SecurityApplicationName
       ,case
          when (select sa.applicationtype from security_application sa where sa.applicationid = ah.securityapplicationid) = 0 then 'ServerApplication'
          when (select sa.applicationtype from security_application sa where sa.applicationid = ah.securityapplicationid) = 1 then 'PresentationApplication'
        end as SecurityApplicationType
       ,'IsActive' as SecurityApplicationState
       ,ah.actiondatetime
       ,(select sn.displayname from strategy_node sn where sn.nodeid = ah.currentnodeid) CurrentNodeName
       ,(select sn.nodeid from strategy_node sn where sn.nodeid = ah.currentnodeid) CurrentNodeId
       , case 
         when ah.actiontype = 0 then 'Lock'
         when ah.actiontype = 1 then 'UnLock' 
         when ah.actiontype = 3 then 'ComingTime'
         when ah.actiontype = 4 then 'ExitTime'
        end as actionType 
    from
      application_history ah
      ,security_user su
    where
      su.userid = ah.userid
      and ah.applicationid = iAppId
    order by
          ah.apphistoryid;
 return l_cur;
end GetHistoryByApplicationId;
/
