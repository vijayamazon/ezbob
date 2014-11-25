IF OBJECT_ID('LoadAutoRerejectConfigs') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoRerejectConfigs AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoRerejectConfigs
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		v.Name,
		v.Value
	FROM
		ConfigurationVariables v
	WHERE
		v.Name LIKE 'AutoReReject%'
END
GO
