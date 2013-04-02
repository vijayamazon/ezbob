IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Lock]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_Lock]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Application_Lock]
	@pApplicationId bigint,
	@pUserId int,
	@pVersion int,
	@pSecurityAppId int -- Add AG
AS
BEGIN
	declare @flag int
	declare @CurrUserId int
	declare @CurrVersion int

	select
	   @CurrUserId = LockedByUserId
	 , @CurrVersion = [Version]
	from Application_Application WITH (NOLOCK)
	where ApplicationId = @pApplicationID

	if ISNULL(@CurrUserId, -1) != @pUserId
	begin
		SET NOCOUNT ON;
		if (@pVersion = -1) or (ISNULL(@CurrVersion, -1) = @pVersion)
		begin
			update Application_Application set
					LockedByUserId = @pUserId
			where ApplicationId = @pApplicationID
			and LockedByUserId is null
			if @@ROWCOUNT = 0
			execute App_RaiseAppNotExistError @pApplicationId
		end
		else
			RAISERROR('ApplicationWasProcessed', 11, 1)
	end

execute app_history_insert @pUserId, @pSecurityAppId, 0, @pApplicationId --Add AG
END
GO
