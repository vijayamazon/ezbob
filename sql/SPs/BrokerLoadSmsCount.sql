IF OBJECT_ID('BrokerLoadSmsCount') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadSmsCount AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadSmsCount
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Name,
		Value
	FROM
		ConfigurationVariables
	WHERE
		Name IN ('BrokerMaxPerNumber', 'BrokerMaxPerPage')
END
GO
