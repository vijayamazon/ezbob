IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyEngine_ExecStatePop]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyEngine_ExecStatePop]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyEngine_ExecStatePop] 
	(@pApplicationId bigint)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @id int;
    
	SELECT @id = MAX(ID)
	FROM [StrategyEngine_ExecutionState] 
	WHERE [ApplicationId] = @pApplicationId;

	SELECT 
	   s.[Data],
	   s.CurrentNodePostfix as CurrentNodeName,
	   a.ExecutionPath as ExecutionPath,
	   a.ExecutionPathBin as ExecutionPathBin
	FROM [StrategyEngine_ExecutionState] s
	     INNER JOIN [Application_Application] a WITH (NOLOCK)
	     	on s.ApplicationId = a.ApplicationId
        WHERE s.Id = @id;

	DELETE 
	FROM [StrategyEngine_ExecutionState]
	WHERE id = @id
END
GO
