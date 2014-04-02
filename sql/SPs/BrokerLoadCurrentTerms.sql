IF OBJECT_ID('BrokerLoadCurrentTerms') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCurrentTerms AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCurrentTerms
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		BrokerTermsID,
		BrokerTerms
	FROM
		BrokerTerms
	ORDER BY
		DateAdded DESC
END
GO
