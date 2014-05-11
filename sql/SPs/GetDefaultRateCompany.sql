IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetConfigTableValue]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetConfigTableValue]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetConfigTableValue] 
	(@ConfigTableName VARCHAR(100),
	 @Key INT)
AS
BEGIN
	DECLARE @Stmnt VARCHAR(MAX)
	SET @Stmnt = 'SELECT Value FROM ' + @ConfigTableName + ' WHERE ' + CONVERT(VARCHAR(10), @Key) + ' >= Start AND ' + CONVERT(VARCHAR(10), @Key) + ' <= [End]'
	EXECUTE(@Stmnt)
END
GO
