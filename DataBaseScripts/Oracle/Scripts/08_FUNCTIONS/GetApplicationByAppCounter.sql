create or replace function GetApplicationByAppCounter(iAppCounter in number) return sys_refcursor
is
 l_cur sys_refcursor;
begin
 open l_cur for
     select
      aa.applicationid
     ,aa.parentappid
     ,aa.appcounter
     ,aa.creationdate
     ,ss.displayname as strategyname
     ,ss.strategyId
     ,cpp.name as creditproductname
     ,Version
     ,NVL(aa.childcount, 0) as childcount
     ,aa.state   
     ,ae.ErrorMessage as errorMessage     
     ,su.userid as userId
     ,su.username as userName
     ,su.fullname as userFullName
     ,su1.userid as LockedByUserId
     ,su1.username as LockedByUserName
     ,su1.fullname as LockedByUserFullName
     ,(select max(ah.actiondatetime) from application_history ah
              where ah.applicationid = aa.applicationid and ah.userid = aa.lockedbyuserid and ah.currentnodeid = sn.nodeid and ah.actiontype = 0) as LockedDate
     ,sn.nodeid as NodeId
     ,sn.name as NodeName
     ,sn.displayname as NodeDisplayName
     ,NVL(
       (select 1
        from Application_Application aa1     
        where aa1.ApplicationId = aa.ApplicationId
            and (aa1.state = 3 OR aa1.state = 4 OR aa1.state = 5 OR aa1.state = 6  OR aa1.state = 7 )     
        ), 0) as CanRestart     
     ,(select count(surr.RoleId)
     from Security_UserRoleRelation surr 
     where surr.UserId = su.UserId
      ) as RolesCount
    from
     application_application aa
     left join security_user su on su.userid = aa.creatoruserid
     left join strategy_strategy ss on ss.strategyid = aa.strategyid
     left join creditproduct_strategyrel cps on cps.strategyid = aa.strategyid
     left join creditproduct_products cpp on cpp.id = cps.creditproductid 
     left join security_user su1 on su1.userid = aa.lockedbyuserid
     left join strategyengine_executionstate se on aa.applicationid = se.applicationid
     left join strategy_node sn on sn.nodeid = se.currentnodeid
     left join Application_Error ae on ae.applicationid = aa.applicationid
    where
     aa.appcounter = iAppCounter and aa.ParentAppId is null;

 return l_cur;
end GetApplicationByAppCounter;
/


