IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateApplicationStrategyId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateApplicationStrategyId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateApplicationStrategyId]
      @pApplicationId [bigint],
      @pNewStrategyId [bigint]
AS	
BEGIN
   UPDATE [dbo].[Application_Application]
      SET [StrategyId] = @pNewStrategyId
   WHERE ApplicationId = @pApplicationId;
END
GO
