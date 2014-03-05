IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChangeLockedUser]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[ChangeLockedUser]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ChangeLockedUser] 
	(@pAppID bigint,
  @pUserID int)
AS
BEGIN
	update application_application set lockedbyuserid = @pUserID where applicationid = @pAppID
END
GO
