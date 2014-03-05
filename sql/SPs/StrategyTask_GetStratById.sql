IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_GetStratById]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_GetStratById]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_GetStratById] 
	(@pStrategyId int)
AS
BEGIN
	SELECT
    DisplayName as [Name], Xml
  FROM
    Strategy_Strategy
  WHERE
    Strategy_Strategy.StrategyId = @pStrategyId
END
GO
