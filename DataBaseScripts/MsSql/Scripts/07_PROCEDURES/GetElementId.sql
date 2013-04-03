IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetElementId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetElementId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetElementId]
      @pApplicationId  bigint
    , @name nvarchar(max)	
AS	
BEGIN
	SELECT ad.DetailId
	FROM Application_Detail ad 
	INNER JOIN Application_DetailName dn ON ad.DetailNameId = dn.DetailNameId
	WHERE
		dn.name = @name AND
		ad.ApplicationId = @pApplicationId
    RETURN    
END
GO
