IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppDetail_UpdateAttach]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AppDetail_UpdateAttach]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	Nataly Yakovenko
-- Create date: 20.12.2007
-- Description:	Updates detail in Application_Detail 
--              (for attachment properties)
-- =============================================
CREATE PROCEDURE [dbo].[AppDetail_UpdateAttach]
	@pApplicationId		BIGINT,
	@pParentDetailId	BIGINT,
	@pDetailName		NVARCHAR( 255),
	@pNewValueStr       NVARCHAR( 512)
	
AS
BEGIN
	DECLARE @DetailIdUpdate BIGINT; 

	SELECT @DetailIdUpdate = ad.[DetailId] 
	FROM [Application_Detail] ad
	INNER JOIN [Application_DetailName] adn 
			ON ad.[DetailNameId] = adn.[DetailNameId]
	WHERE	ad.[ApplicationId] = @pApplicationId
			and ad.[ParentDetailId] = @pParentDetailId
			and adn.[Name] = @pDetailName 

	UPDATE [Application_Detail]
	SET [ValueStr] = @pNewValueStr
	WHERE [DetailId] = @DetailIdUpdate
	
END
GO
