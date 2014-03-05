IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailSelectChildIDs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailSelectChildIDs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_DetailSelectChildIDs] 
	(@pName NVARCHAR(255),
    @pApplicationId BIGINT,
	@pParentDetailID BIGINT)
AS
BEGIN
	IF @pParentDetailID IS NULL 
    SELECT [Application_Detail].[DetailID] DetailID
      FROM [Application_Detail] 
      JOIN [Application_DetailName]ON [Application_Detail].[DetailNameID] = [Application_DetailName].[DetailNameId] 
     WHERE [Application_DetailName].[Name] = @pName 
       AND [Application_Detail].[ApplicationID] = @pApplicationId 
       AND [Application_Detail].[ParentDetailID] IS NULL 
     ORDER BY DetailID 
     
    ELSE 
     
    SELECT [Application_Detail].[DetailID] DetailID
      FROM [Application_Detail] 
      JOIN [Application_DetailName]ON [Application_Detail].[DetailNameID] = [Application_DetailName].[DetailNameId] 
     WHERE [Application_DetailName].[Name] = @pName 
       AND [Application_Detail].[ApplicationID] = @pApplicationId 
       AND [Application_Detail].[ParentDetailID] = @pParentDetailID 
     ORDER BY DetailID
END
GO
