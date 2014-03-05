IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_ExecutionPathUpdate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_ExecutionPathUpdate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_ExecutionPathUpdate] 
	(@pApplicationId  BIGINT,
	@pExecutionPathBin  varbinary(max))
AS
BEGIN
	UPDATE Application_Application
    SET ExecutionPathBin = @pExecutionPathBin, ExecutionPath = null
    WHERE ApplicationId = @pApplicationId;
END
GO
