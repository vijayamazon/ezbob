IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateIsTimeoutReported]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateIsTimeoutReported]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateIsTimeoutReported] 
	(@pExecutionStateId bigint)
AS
BEGIN
	UPDATE STRATEGYENGINE_EXECUTIONSTATE
   SET ISTIMEOUTREPORTED = 1
   WHERE STRATEGYENGINE_EXECUTIONSTATE.ID = @pExecutionStateId
END
GO
