IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_RaiseAppNotExistError]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_RaiseAppNotExistError]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_RaiseAppNotExistError] 
	(@ApplicationId bigint)
AS
BEGIN
	declare @LockedByUser int
     set @LockedByUser = null

     select @LockedByUser = LockedByUserId
     from Application_Application
     where ApplicationId = @ApplicationID

     if @LockedByUser is null
        RAISERROR('ApplicationIdDoesNotExist', 11, 1)
      else
        RAISERROR('ApplicationLockedByOtherUser', 11, 1)
END
GO
