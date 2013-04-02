IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuspendApplicationById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SuspendApplicationById]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [SuspendApplicationById]
	@pApplicationId BIGINT,
	@pExecutionPathCurrentItemId BIGINT
AS
BEGIN
	DECLARE @pExecutionState  NVARCHAR(MAX);
	DECLARE @pPostfix         NVARCHAR(MAX);
	
	SELECT @pExecutionState = DATA,
	       @pPostfix = CurrentNodePostfix
	FROM   StrategyEngine_ExecutionState
	WHERE  applicationId = @pApplicationId;
	
	DECLARE @pTarget         NVARCHAR(MAX);
	DECLARE @pLabel          NVARCHAR(MAX);
	DECLARE @pMessage        VARBINARY(MAX);
	DECLARE @pExecutionType  NVARCHAR(MAX);
	
	SELECT @pTarget = s.Target,
	       @pLabel = s.Label,
	       @pMessage = s.Message,
	       @pExecutionType = s.ExecutionType
	FROM   Signal s
	WHERE  s.applicationId = @pApplicationId;
	
	DECLARE @pAppSpecific  BIGINT;
	DECLARE @pStrategyId   INT;
	
	EXEC DeleteSignalByAppId @pApplicationId,
	     @pAppSpecific OUTPUT,
	     @pStrategyId OUTPUT;
	
	DECLARE @pCurrentNodeId INT;
	EXEC DeleteExecutionStateByAppId @pApplicationId,
	     @pCurrentNodeId OUTPUT;
	
	DELETE FROM Application_Suspended
	WHERE [ApplicationId] = @pApplicationId;
	INSERT INTO [Application_Suspended]
	  (
	    [ApplicationId],
	    [ExecutionState],
	    [Postfix],
	    [Target],
	    [Label],
	    [ExecutionPathCurrentItemId],
	    [Message],
	    [AppSpecific],
	    [ExecutionType],
	    [Date]
	  )
	VALUES
	  (
	    @pApplicationId,
	    @pExecutionState,
	    @pPostfix,
	    @pTarget,
	    @pLabel,
	    @pExecutionPathCurrentItemId,
	    @pMessage,
	    @pAppSpecific,
	    @pExecutionType,
	    GETDATE()
	  );
	
	SELECT @@IDENTITY;
END;
GO
