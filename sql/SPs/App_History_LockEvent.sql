IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_LockEvent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_LockEvent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_History_LockEvent] 
	(@pUserId int,
 @pSecurityAppId int,
 @pApplicationId int)
AS
BEGIN
	DECLARE  @currentTime datetime,
			 @comingTime datetime,
			 @currentNodeID int,
			 @currentSummaryRowId int,
			 @currentNodeTimeId int,
			 @lastLockDate datetime,
			 @lastUnlockDate datetime,
			 @delta int;
   SET @currentTime = GETDATE();

   --get current node id
   SELECT @currentNodeID=currentnodeid
          FROM strategyengine_executionstate
         WHERE applicationId = @pApplicationId;

     -- get current node time
     select @currentNodeTimeId=id, @comingTime=comingtime 
     from application_nodetime
           where applicationId = @pApplicationId  and nodeid = @currentNodeID
           and firsttimeoutage is null and exittime is null;

    -- get current summary row id
          select @currentSummaryRowId=id  
            from application_historysumm
           where applicationId = @pApplicationId and nodeid = @currentNodeID;
      
	IF @currentSummaryRowId IS NULL RETURN;

    SET @delta = DATEDIFF(second , @comingTime, @currentTime);
	
	IF @currentNodeTimeId IS NOT NULL
	BEGIN
		update application_nodetime
		   set firsttimeoutage = @delta,
		   userid = @pUserId,
		   securityapplicationid = @pSecurityAppId
		   where id = @currentNodeTimeId;

		update application_historysumm
		   set summfirsttimeoutage = ISNULL(summfirsttimeoutage, 0)+@delta,
		   generaloutagetime = ISNULL(generaloutagetime, 0) + @delta,
		   securityapplicationid = @pSecurityAppId
		   where id = @currentSummaryRowId;
	END

	ELSE
		BEGIN

         select @currentNodeTimeId=id
                from application_nodetime
         where applicationId = @pApplicationId and securityapplicationid = @pSecurityAppId
         and nodeid = @currentNodeID and userid = @pUserId and exittime is null;

         -- get current summary row id
         select @currentSummaryRowId=id  
                from application_historysumm
               where applicationId = @pApplicationId and securityapplicationid = @pSecurityAppId
               and nodeid = @currentNodeID;

         IF @currentSummaryRowId IS NULL RETURN;

         --get last lock replace null with min date value, used ugly code, needed refactoring
         select @lastLockDate = MAX(actiondatetime) from application_history
         where applicationid = @pApplicationId and actiontype=0 and userid = @pUserId
        and currentnodeid= @currentNodeID and securityapplicationid = @pSecurityAppId;
        --get last unlock
        select @lastUnlockDate = MAX(actiondatetime) from application_history
        where applicationid = @pApplicationId and actiontype=1 and userid = @pUserId
        and currentnodeid= @currentNodeID and securityapplicationid = @pSecurityAppId;

        IF @lastLockDate > @lastUnlockDate 
           SET @delta = DATEDIFF(second, @lastLockDate, @currentTime);
        ELSE
           SET @delta = DATEDIFF(second, @lastUnlockDate, @currentTime);

       update application_nodetime
       set outagetimelockunlock = ISNULL(outagetimelockunlock, 0)+@delta
       where id = @currentNodeTimeId;

       update application_historysumm
       set  summoutagetimelockunlock = ISNULL(summoutagetimelockunlock, 0)+@delta,
       generaloutagetime = ISNULL(generaloutagetime, 0) + @delta
       where id = @currentSummaryRowId;
   END;
END
GO
