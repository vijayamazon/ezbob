IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'UnderwriterSite')
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UnderwriterSite', '192.168.120.3', 'Underwriter site host and port. Used in reports to open customer profile')
GO

IF OBJECT_ID('LoadConfigurationVariable') IS NULL
	EXECUTE('CREATE PROCEDURE LoadConfigurationVariable AS SELECT 1')
GO

ALTER PROCEDURE LoadConfigurationVariable
@CfgVarName NVARCHAR(256)
AS
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
GO
