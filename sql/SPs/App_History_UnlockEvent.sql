IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_UnlockEvent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_UnlockEvent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_History_UnlockEvent] 
	(@pUserId int,
 @pSecurityAppId int,
 @pApplicationId int)
AS
BEGIN
	DECLARE  @currentTime datetime,
			 @currentNodeID int,
			 @currentSummaryRowId int,
			 @currentNodeTimeId int,
			 @lastLockDate datetime,
			 @delta int;

   SET @currentTime = GETDATE();

   --get current node id
   SELECT @currentNodeID=currentnodeid
          FROM strategyengine_executionstate
         WHERE applicationId = @pApplicationId;

   -- get current summary row id
       select @currentSummaryRowId=id  
          from application_historysumm
         where applicationId = @pApplicationId and securityapplicationid = @pSecurityAppId
         and nodeid = @currentNodeID;
    
	IF @currentSummaryRowId IS NULL RETURN;

   -- get current node time row Id
     select @currentNodeTimeId=id  
     from application_nodetime
           where applicationId = @pApplicationId and securityapplicationid = @pSecurityAppId
           and nodeid = @currentNodeID and exittime is null;
	
	IF @currentNodeTimeId IS NULL RETURN;

       --get last lock
       select @lastLockDate = MAX(actiondatetime) from application_history
       where applicationid = @pApplicationId and actiontype=0 and userid = @pUserId
       and currentnodeid= @currentNodeID and securityapplicationid = @pSecurityAppId;

       SET @delta = DATEDIFF(second, @lastLockDate, @currentTime);

       update application_nodetime
       set worktime =ISNULL(worktime, 0) +@delta
       where id = @currentNodeTimeId;

       update application_historysumm
       set summworktime  = ISNULL(summworktime, 0) +@delta
       where id = @currentSummaryRowId;
END
GO
