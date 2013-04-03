IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_SchemaSelect]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_SchemaSelect]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Strategy_SchemaSelect] 
@pStrategyId AS INT

AS
BEGIN
	SELECT * FROM [Strategy_Schemas]
	WHERE StrategyId = @pStrategyId;
END
GO
