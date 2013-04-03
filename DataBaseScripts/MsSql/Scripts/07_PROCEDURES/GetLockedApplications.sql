IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLockedApplications]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLockedApplications]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLockedApplications]
AS
BEGIN
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
END;
GO
