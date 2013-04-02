IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyEngine_ExecStatePush]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyEngine_ExecStatePush]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyEngine_ExecStatePush] 
	@pExecutionState ntext,
	@pCurrentNodeType nvarchar(max),
	@pCurrentNodePostfix nvarchar(max),
	@pApplicationId bigint,
	@pExecutionPathBin varbinary(max)
AS
BEGIN
	SET NOCOUNT ON;
	
    declare @CurrentNodeId int;
	
    SELECT @CurrentNodeId = Strategy_Node.NodeId
    FROM   Strategy_Node INNER JOIN
           Strategy_NodeStrategyRel ON Strategy_Node.NodeId = Strategy_NodeStrategyRel.NodeId INNER JOIN
           Application_Application ON Strategy_NodeStrategyRel.StrategyId = Application_Application.StrategyId
    WHERE Application_Application.ApplicationId = @pApplicationId and Strategy_Node.[Name]+Strategy_Node.[Guid] = @pCurrentNodeType;

    execute App_ExecutionPathUpdate @pApplicationId, @pExecutionPathBin
   
    INSERT INTO [StrategyEngine_ExecutionState]( [Data], [ApplicationId], CurrentNodeId, CurrentNodePostfix)
    VALUES( @pExecutionState, @pApplicationId, @CurrentNodeId, @pCurrentNodePostfix)

END
GO
