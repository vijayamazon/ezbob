IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AddLink]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AddLink]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddLink] 
	(@pEntityId   [bigint],
     @pSeriaId    [bigint],
     @pUserId     [bigint],
     @pEntityType [nvarchar](100),
     @pLinksDoc   [nvarchar](max),
     @pSignedDoc  [nvarchar](max),
     @pIsApproved [bit])
AS
BEGIN
	INSERT INTO [EntityLink]
             ([SeriaId]
             ,[EntityType]
             ,[EntityId]
             ,[UserId]
             ,[LinksDoc]
             ,[SignedDoc]
             ,[IsApproved])
       VALUES
             (@pSeriaId
             ,@pEntityType
             ,@pEntityId
             ,@pUserId
             ,@pLinksDoc
             ,@pSignedDoc
             ,@pIsApproved)

	SELECT @@IDENTITY
	RETURN
END
GO
