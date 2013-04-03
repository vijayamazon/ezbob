IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteLinksBySourceId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteLinksBySourceId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteLinksBySourceId]
      @pLinkedFrom  [bigint],
      @pEntityType [nvarchar](100)
AS	
BEGIN
 UPDATE [dbo].[EntityLink]
   SET [IsDeleted] = 1
 WHERE EntityId = @pLinkedFrom OR SeriaId=@pLinkedFrom
   AND EntityType = @pEntityType;
END
GO
