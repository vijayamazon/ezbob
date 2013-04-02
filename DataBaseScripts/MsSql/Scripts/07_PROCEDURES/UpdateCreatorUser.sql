IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCreatorUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateCreatorUser]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCreatorUser]
(
  @pCreatorUserId int,
  @pAppId bigint
)
AS
BEGIN
     Update Application_Application
     set CreatorUserId = @pCreatorUserId
     where ApplicationId=@pAppId;
END;
GO
