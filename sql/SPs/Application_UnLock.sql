IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_UnLock]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_UnLock]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Application_UnLock] 
	(@pApplicationId bigint,
	@pState int,
	@pIsVersion bit, /* Indicates if Version of Application must be updated*/
    @pSecurityAppId int,
    @pAppNewVersion int output)
AS
BEGIN
	SET NOCOUNT ON;

	/*Updates version of Application*/
	if @pIsVersion = 1
	begin
	declare @CurrVersion int
	select @CurrVersion = Version from Application_Application 
		where ApplicationId = @pApplicationID
	update Application_Application set  Version = @CurrVersion + 1
        where ApplicationId = @pApplicationID
	end


declare @lUserId int
select @lUserId = lockedbyuserid from Application_Application 
		where ApplicationId = @pApplicationID

    if @pState is null
        update Application_Application set
           LockedByUserId = null
        where ApplicationId = @pApplicationID
    else
        update Application_Application set
           LockedByUserId = null,
           State = @pState
        where ApplicationId = @pApplicationID

execute app_history_insert @lUserId, @pSecurityAppId, 1, @pApplicationId --Add AG


select @pAppNewVersion = Version from Application_Application where ApplicationId = @pApplicationID
END
GO
