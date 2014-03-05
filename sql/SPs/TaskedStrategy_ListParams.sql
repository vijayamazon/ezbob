IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaskedStrategy_ListParams]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[TaskedStrategy_ListParams]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[TaskedStrategy_ListParams] 
	(@pTaskedStrategyId int)
AS
BEGIN
	SELECT *
  FROM TaskedStrategyParams
  WHERE TSId = @pTaskedStrategyId
END
GO
