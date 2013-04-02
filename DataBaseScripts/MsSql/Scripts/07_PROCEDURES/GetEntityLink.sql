IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEntityLink]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetEntityLink]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetEntityLink]
      @pEntityId   [bigint],
      @pEntityType [nvarchar](100),
      @pIsApproved [bit]
AS	
BEGIN
  SELECT LinksDoc AS DescriptionDocument
  FROM EntityLink
  WHERE EntityType = @pEntityType
    AND EntityId = @pEntityId
    AND SeriaId = (
      SELECT MAX(SeriaId)
      FROM EntityLink
      WHERE EntityType = @pEntityType
        AND EntityId = @pEntityId)
        AND (IsDeleted IS null OR IsDeleted = 0)
        AND (   (IsApproved = @pIsApproved 
                 AND NOT (@pIsApproved is null))
             OR (@pIsApproved is null));

    RETURN    
END
GO
