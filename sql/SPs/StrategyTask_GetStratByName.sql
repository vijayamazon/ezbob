IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_GetStratByName]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_GetStratByName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_GetStratByName] 
	(@pStrategyName nvarchar(MAX),
  @pStrategyType nvarchar(255))
AS
BEGIN
	SELECT
    ts.Id, ts.TaskId, ts.Label, ts.StrategyId, st.DisplayName as [Name], st.CurrentVersionId as Version
  FROM
    TaskedStrategies ts, Strategy_Strategy st
  WHERE
    ts.StrategyId = st.StrategyId
    and st.DisplayName = @pStrategyName
    and st.StrategyType = @pStrategyType
    and st.isdeleted = 0
END
GO
