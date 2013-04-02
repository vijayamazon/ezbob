create or replace function GetApplicationById(iApplicationId in number) return  sys_refcursor
is
 l_cur sys_refcursor;
begin
 open l_cur for

select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,aa.strategyid
     ,ss.displayname as strategyname
     ,cpp.name as creditproduct_name
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
     
     ,susp."Date" as ActionDate,
     (select count(surr.RoleId)
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
     left join Application_Suspended susp on aa.applicationid = susp.applicationid and (aa.state = 5 OR aa.state = 6  OR aa.state = 7)
    where
     aa.applicationid = iApplicationId;

 return l_cur;
end GetApplicationById;
/
