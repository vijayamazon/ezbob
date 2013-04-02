IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnInspector_GetTuskListOld]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnInspector_GetTuskListOld]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[fnInspector_GetTuskListOld]
(
	@UserId int
)
RETURNS 
TABLE 
AS
return(
select a.ApplicationId, a.Param1, a.Param2, n.Name CurrentNodeName, n.NodeId CurrentNodeId, t.Name StrategyName, Version from application_Application a, Strategy_Node n,
StrategyEngine_ExecutionState s, Strategy_Strategy t,
(SELECT max(id) as id, applicationId
 FROM [StrategyEngine_ExecutionState]
 Group by applicationId) ids
 where ids.id = s.id and ids.applicationId = a.applicationId
 and n.NodeId = s.CurrentNodeId
 and (a.LockedByUserId is null or a.LockedByUserId = @UserId)
and t.StrategyId = a.StrategyId and a.state = 1
)
GO
