IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteExecutionStateByAppId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteExecutionStateByAppId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteExecutionStateByAppId]
	@pApplicationId         BIGINT,
	@pCurrentNodeId			INT OUTPUT
AS
BEGIN
	SELECT @pCurrentNodeId=se.currentnodeid FROM StrategyEngine_ExecutionState se WHERE se.applicationid = @pApplicationId;      
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED 
	DELETE FROM StrategyEngine_ExecutionState WHERE applicationId = @pApplicationId;
	
	select @pCurrentNodeId;
END;
GO
