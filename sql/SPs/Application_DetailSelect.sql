IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_DetailSelect]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_DetailSelect]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Application_DetailSelect] 
	(@pApplicationId BIGINT)
AS
BEGIN
	SELECT ad.[DetailId]       DetailId
	   ,ad.[ParentDetailId] ParentDetailId
	   ,ad.[ValueStr]       ValueStr
	   ,ad.[ValueNum]       ValueNum
	   ,ad.[ValueDateTime]  ValueDateTime
	   ,ad.[IsBinary]       IsBinary
	   ,adn.[Name]          Name
	FROM [Application_Detail] ad 
	LEFT JOIN [Application_DetailName] adn ON adn.[DetailNameId] = ad.[DetailNameId] 
	WHERE [ApplicationId] = @pApplicationId 
	ORDER BY ad.[ParentDetailID] ;
END
GO
