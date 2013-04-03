IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_History_Insert]
	@pUserId        int,
    @pSequrityAppId int,
    @pActionType    int,
    @pApplicationId bigint
AS
BEGIN
	SET NOCOUNT ON;

        DECLARE @CurrentNodeID INTEGER;
    SELECT @CurrentNodeID = CurrentNodeID 
      FROM StratEgyengine_ExecutionState 
     WHERE applicationId = @pApplicationId;
	insert into Application_History
    (userid,
     securityapplicationid,
     actiondatetime,
     actiontype,
     applicationid,
     CurrentNodeID
     )

  values
    (@pUserId,
     @pSequrityAppid,
     getdate(),
     @pActionType,
     @pApplicationId,
     @CurrentNodeID)

END
GO
