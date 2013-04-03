IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyEngine_ExecStatePeek]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyEngine_ExecStatePeek]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyEngine_ExecStatePeek]
	@pApplicationId bigint
AS
BEGIN

	SET NOCOUNT ON;
	DECLARE @id int;
    
	SELECT @id = MAX(ID)
	FROM [StrategyEngine_ExecutionState] 
	WHERE [ApplicationId] = @pApplicationId;

	SELECT s.[Data], s.CurrentNodePostfix as CurrentNodeName, a.ExecutionPathBin as ExecutionPathBin
	FROM [StrategyEngine_ExecutionState] s
	     left outer join [Strategy_Node] n WITH (NOLOCK)
	     	on n.NodeId = s.CurrentNodeId
	     INNER JOIN [Application_Application] a WITH (NOLOCK)
	     	on s.ApplicationId = a.ApplicationId
        WHERE s.Id = @id;
END
GO
