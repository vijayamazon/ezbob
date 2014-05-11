IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetConfigTable]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetConfigTable]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetConfigTable]
	(@TableName VARCHAR(100))
AS
BEGIN
	EXECUTE('SELECT Id, Start, [End], Value FROM ' + @TableName)
END
GO
