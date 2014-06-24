IF OBJECT_ID('GetAllConfigurationVariables') IS NULL
	EXECUTE('CREATE PROCEDURE GetAllConfigurationVariables AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetAllConfigurationVariables
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Id AS ID,
		Name,
		Value,
		Description,
		ISNULL(IsEncrypted, 0) AS IsEncrypted
	FROM
		ConfigurationVariables
END
GO
