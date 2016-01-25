SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueRemoteRequestsAreEnabled') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueRemoteRequestsAreEnabled AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueRemoteRequestsAreEnabled
@Value NVARCHAR(255) OUTPUT
AS
BEGIN
	SELECT
		@Value = Value
	FROM
		ConfigurationVariables
	WHERE
		Name = 'LogicalGlueEnabled'
END
GO
