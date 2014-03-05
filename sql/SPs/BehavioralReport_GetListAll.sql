IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BehavioralReport_GetListAll]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BehavioralReport_GetListAll]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BehavioralReport_GetListAll] 
AS
BEGIN
	SELECT rep.Id, rep.StrategyId, rep.Name, s.DisplayName as StrategyName,
         rep.TypeId, rep.CreationDate, rep.TestRun, rep.IsNotRead
  FROM BehavioralReports rep, Strategy_Strategy s
  WHERE rep.StrategyId = s.StrategyId
END
GO
