IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadConfigurationVariable]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadConfigurationVariable]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LoadConfigurationVariable] 
	(@CfgVarName NVARCHAR(256))
AS
BEGIN
	SELECT
		cv.Name,
		cv.Value,
		cv.Description
	FROM
		ConfigurationVariables cv
	WHERE
		cv.Name LIKE @CfgVarName
	ORDER BY
		cv.Name
END
GO
