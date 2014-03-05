IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_GetStrategies]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_GetStrategies]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_GetStrategies] 
	(@pTaskId int,
  @pStrategyType nvarchar(255))
AS
BEGIN
	SELECT
    ts.Id, ts.TaskId, ts.Label, ts.StrategyId, st.DisplayName as [Name], st.CurrentVersionId as Version
  FROM
    TaskedStrategies ts, Strategy_Strategy st
  WHERE
    ts.StrategyId = st.StrategyId
    and ts.TaskId = @pTaskId
    and st.StrategyType = @pStrategyType
    and st.isdeleted = 0
END
GO
