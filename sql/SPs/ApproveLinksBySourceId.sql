IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApproveLinksBySourceId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[ApproveLinksBySourceId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ApproveLinksBySourceId] 
	(@pLinkedFrom   [bigint]
      ,@pEntityType [nvarchar](100)
      ,@pIsApproved [bigint])
AS
BEGIN
	UPDATE [dbo].[EntityLink]
   SET [IsApproved] = @pIsApproved
 WHERE EntityId = @pLinkedFrom
   AND EntityType = @pEntityType
   AND (EntityLink.IsDeleted = 0 or EntityLink.IsDeleted IS null);
END
GO
