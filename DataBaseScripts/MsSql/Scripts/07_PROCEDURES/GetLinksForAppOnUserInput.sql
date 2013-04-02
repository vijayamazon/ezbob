IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLinksForAppOnUserInput]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLinksForAppOnUserInput]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLinksForAppOnUserInput]
       @pApplicationId [bigint]
      ,@pEntityType [nvarchar](100)
AS	
BEGIN
SELECT
    StrategyEngine_ExecutionState.CurrentNodePostfix AS NodeName
FROM  StrategyEngine_ExecutionState INNER JOIN
   Application_Application ON StrategyEngine_ExecutionState.ApplicationId = Application_Application.ApplicationId
    INNER JOIN EntityLink ON Application_Application.StrategyId = EntityLink.EntityId
WHERE (EntityLink.IsDeleted = 0 OR EntityLink.IsDeleted is null) 
  AND (EntityLink.IsApproved = 1)
  AND (EntityType = @pEntityType)
  AND (StrategyEngine_ExecutionState.ApplicationId = @pApplicationId);
END
GO
