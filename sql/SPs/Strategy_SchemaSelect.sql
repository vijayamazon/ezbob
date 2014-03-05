IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_SchemaSelect]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_SchemaSelect]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_SchemaSelect] 
	(@pStrategyId INT)
AS
BEGIN
	SELECT * FROM [Strategy_Schemas]
	WHERE StrategyId = @pStrategyId
END
GO
