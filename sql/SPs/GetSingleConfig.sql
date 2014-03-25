IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSingleConfig]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSingleConfig]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSingleConfig] 
	(@ConfigName NVARCHAR(256))
AS
BEGIN
	SELECT 
		Value 
	FROM 
		ConfigurationVariables 
	WHERE 
		Name = @ConfigName
END
GO
