IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SE_ExecStateStackDepth]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SE_ExecStateStackDepth]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SE_ExecStateStackDepth] 
	(@pStackDepth int OUTPUT,
   @pApplicationId bigint)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT @pStackDepth = COUNT(*) FROM [StrategyEngine_ExecutionState] 
    WHERE ApplicationId = @pApplicationId
END
GO
