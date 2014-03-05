IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyScheduleParam_Delete]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyScheduleParam_Delete]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyScheduleParam_Delete] 
	(@pName  nvarchar(100),
    @pScheduleId int)
AS
BEGIN
	UPDATE [Strategy_ScheduleParam]
	SET [Deleted] = [Id]
	WHERE [Name] = @pName
      AND Deleted IS null
      AND StrategyScheduleId = @pScheduleId
END
GO
