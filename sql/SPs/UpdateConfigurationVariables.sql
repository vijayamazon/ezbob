IF OBJECT_ID('UpdateConfigurationVariables') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateConfigurationVariables AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateConfigurationVariables
@UpdatePackage IntTextList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE ConfigurationVariables SET
		Value = u.Value
	FROM
		ConfigurationVariables c
		INNER JOIN @UpdatePackage u ON c.Id = u.ID
END
GO
