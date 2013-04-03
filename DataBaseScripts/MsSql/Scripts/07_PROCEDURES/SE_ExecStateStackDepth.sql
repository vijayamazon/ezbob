IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SE_ExecStateStackDepth]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SE_ExecStateStackDepth]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SE_ExecStateStackDepth]
	-- Add the parameters for the stored procedure here
   @pStackDepth int OUTPUT,
   @pApplicationId bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT @pStackDepth = COUNT(*) FROM [StrategyEngine_ExecutionState] 
    WHERE ApplicationId = @pApplicationId;
END
GO
