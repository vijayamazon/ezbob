IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_ComingEvent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_ComingEvent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_History_ComingEvent]

(@pUserId        int,
 @pSecurityAppId int,
 @pApplicationId int)

 AS
   
BEGIN
   DECLARE @lCurrentNodeID as int,
   @currentDate as datetime;


   SET @currentDate = GETDATE();

   SELECT @lCurrentNodeID = currentnodeid
          FROM strategyengine_executionstate
         WHERE applicationId = @pApplicationId;


  -- Add coming time
  insert into Application_History
    (userid, securityapplicationid, actiondatetime,  actiontype,   applicationid, currentnodeid )
  values
    (@pUserId, @pSecurityAppid, @currentDate, 3,  @pApplicationId,  @lCurrentNodeID );
  -- Create new row history node time
  insert into application_nodetime
    (applicationid, nodeid, comingtime)
  values
    (@pApplicationId, @lCurrentNodeID, @currentDate);
  -- if needed add new row to app history summ table
  IF NOT EXISTS(select id 
      from application_historysumm
     where applicationid = @pApplicationId and nodeid = @lCurrentNodeID)
	
  BEGIN
         insert into application_historysumm
           (applicationid, nodeid, firstcomingtime)
         values
           (@pApplicationId, @lCurrentNodeID,  @currentDate);
  END


END
GO
