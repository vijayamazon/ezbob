IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_ExitEvent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_ExitEvent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_History_ExitEvent] 
	(@pUserId int,
 @pSecurityAppId int,
 @pApplicationId int)
AS
BEGIN
	DECLARE @currentTime datetime,
	@currentNodeID int,
	@currentSummaryRowId int,
	@currentNodeTimeId int,
	@comingTimeNode datetime,
	@lastLockDate datetime,
	@delta int,
	@summTimeOfFlyAcc int;
	
	
	SET @currentTime = GETDATE();

   --get current node id
   SELECT @currentNodeID = currentnodeid
          FROM strategyengine_executionstate
         WHERE applicationId = @pApplicationId;

     -- Add exit time
  insert into APPLICATION_HISTORY
    (userid, securityapplicationid, actiondatetime,  actiontype,   applicationid, currentnodeid )
  values
    (@pUserId, @pSecurityAppid, @currentTime, 4,  @pApplicationId,  @currentNodeID );

	IF @pSecurityAppId = -1 
		BEGIN
           update application_nodetime
           set exittime = @currentTime,
           timeoffly = datediff(second, comingtime, @currentTime),
           outagetimelockunlock = datediff(second, comingtime, @currentTime )-ISNULL(worktime, 0) - ISNULL(firsttimeoutage, 0),
           exittype = 1
           where applicationId = @pApplicationId and nodeid = @currentNodeID
           and exittime is null;
           
           
           DECLARE @id int, @securityAppId int;
			DECLARE History_Cursor CURSOR FOR
			select id, securityapplicationid from application_historysumm 
			where applicationId = @pApplicationId and nodeid = @currentNodeID;
			OPEN History_Cursor;
			FETCH NEXT FROM History_Cursor INTO @id, @securityAppId;
			WHILE @@FETCH_STATUS = 0
			   BEGIN
					select @summTimeOfFlyAcc=sum(t.timeoffly) from application_nodetime t
					 where t.applicationid = @pApplicationId and t.nodeid = @currentNodeID and t.securityapplicationid = @securityAppId;
		             
					 update application_historysumm
					 set lastexittime = @currentTime,
					 absolutetimeofflynode = datediff(second, firstcomingtime, @currentTime),
					 summoutagetimelockunlock = @summTimeOfFlyAcc - ISNULL(summworktime, 0) - ISNULL(summfirsttimeoutage, 0),
					 generaloutagetime = @summTimeOfFlyAcc - ISNULL(summworktime, 0),
					 summtimeoffly = @summTimeOfFlyAcc
					 where id = @id;
					 FETCH NEXT FROM History_Cursor INTO @id, @securityAppId;
			   END;
			CLOSE History_Cursor;
			DEALLOCATE History_Cursor;
    END;    

   -- get current summary row id
        select @currentSummaryRowId=id  
          from application_historysumm
         where applicationId = @pApplicationId and securityapplicationid = @pSecurityAppId
         and nodeid = @currentNodeID;

   IF @currentSummaryRowId IS NULL RETURN;

   -- get current node time row Id

     select @currentNodeTimeId=id, @comingTimeNode=comingtime 
     from application_nodetime
           where applicationId = @pApplicationId and securityapplicationid = @pSecurityAppId
           and nodeid = @currentNodeID and exittime is null;

    IF @currentNodeTimeId IS NULL RETURN;

       --get last lock
       select @lastLockDate = MAX(actiondatetime) from application_history
       where applicationid = @pApplicationId and actiontype=0 and userid = @pUserId
       and currentnodeid= @currentNodeID and securityapplicationid = @pSecurityAppId;

       SET @delta = DATEDIFF(second,  @lastLockDate,@currentTime);

       update application_nodetime
       set worktime = ISNULL(worktime, 0) +@delta,
       exittime = @currentTime,
       timeoffly = DATEDIFF(second, @comingTimeNode, @currentTime)
       where id = @currentNodeTimeId;

       update application_historysumm
       set summworktime = ISNULL(summworktime, 0) +@delta,
       lastexittime = @currentTime,
       absolutetimeofflynode = DATEDIFF(second, firstcomingtime, @currentTime),
       SummTimeOfFly = ISNULL(summtimeoffly, 0) + DATEDIFF(second,@comingTimeNode, @currentTime )
       where id = @currentSummaryRowId;
END
GO
