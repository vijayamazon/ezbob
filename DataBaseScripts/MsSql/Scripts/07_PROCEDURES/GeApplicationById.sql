IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GeApplicationById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GeApplicationById]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GeApplicationById]
	@iApplicationId bigint
AS
BEGIN
	select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,ss.displayname as strategyname
     ,cpp.name as creditproduct_name
     ,Version
     ,ISNULL(aa.childcount, 0) as childcount
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
     
     ,susp.date as ActionDate
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
     aa.applicationid = @iApplicationId;
END
GO
