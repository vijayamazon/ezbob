IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEntityLinks]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetEntityLinks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetEntityLinks]
      @pEntitySeriaId  [bigint],
      @pEntityType [nvarchar](100)
AS	
BEGIN
  SELECT LinksDoc AS DescriptionDocument
  FROM EntityLink
  WHERE EntityType = @pEntityType
    AND (   (SeriaId = @pEntitySeriaId 
             AND NOT (@pEntitySeriaId is null))
         OR (@pEntitySeriaId is null));

    RETURN    
END
GO
