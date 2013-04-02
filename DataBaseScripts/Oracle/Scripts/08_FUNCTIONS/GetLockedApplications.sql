create or replace function GetLockedApplications return sys_refcursor
is
 l_cur sys_refcursor;
begin
 open l_cur for
      select
             aa.applicationid,
             aa.appcounter,
             aa.creationdate,
             (select max(ah.actiondatetime) from application_history ah
              where ah.applicationid = aa.applicationid and ah.userid = aa.lockedbyuserid and ah.currentnodeid = sn.nodeid and ah.actiontype = 0) as LockedDate,
             aa.version,
             sn.name as NodeName,
             sn.displayname as NodeDisplayName,
             sn.nodeid as NodeId,
             (SELECT su.userid   FROM security_user su WHERE su.userid = aa.creatoruserid) CreatorUserId,
             (SELECT su.username FROM security_user su WHERE su.userid = aa.creatoruserid) CreatorUserName,
             (SELECT su.fullname FROM security_user su WHERE su.userid = aa.creatoruserid) CreatorUserFullName,
             (SELECT su.userid   FROM security_user su WHERE su.userid = aa.lockedbyuserid) LockedByUserId,
             (SELECT su.username FROM security_user su WHERE su.userid = aa.lockedbyuserid) LockedByUserName,
             (SELECT su.fullname FROM security_user su WHERE su.userid = aa.lockedbyuserid) LockedByUserFullName
        from application_application aa,
             strategyengine_executionstate se,
             strategy_node sn
       where
                 aa.applicationid = se.applicationid
             and sn.nodeid = se.currentnodeid
             and aa.lockedbyuserid is not null;
 return l_cur;
end GetLockedApplications;
/
