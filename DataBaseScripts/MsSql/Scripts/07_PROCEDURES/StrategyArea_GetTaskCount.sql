IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyArea_GetTaskCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyArea_GetTaskCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyArea_GetTaskCount]
(
  @pAreaID int
)
AS
BEGIN

  DECLARE @cnt int;

  SELECT @cnt = COUNT(*) 
    FROM StrategyTasks
    WHERE AreaID = @pAreaId;

  SELECT @cnt;
  RETURN @cnt;

END
GO
