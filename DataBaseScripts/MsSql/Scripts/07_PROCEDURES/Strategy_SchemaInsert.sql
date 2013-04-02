IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_SchemaInsert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_SchemaInsert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_SchemaInsert] 
@pStrategyId AS INT,
@pBinaryData AS VARBINARY(MAX)

AS
BEGIN
	INSERT INTO [Strategy_Schemas]
           ([StrategyId],[BinaryData])
     VALUES
           (@pStrategyId, @pBinaryData)
END
GO
