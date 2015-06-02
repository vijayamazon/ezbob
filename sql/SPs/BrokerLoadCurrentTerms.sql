SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerLoadCurrentTerms') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCurrentTerms AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCurrentTerms
@OriginID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		bt.BrokerTermsID,
		bt.BrokerTerms
	FROM
		BrokerTerms bt
	WHERE
		bt.OriginID = @OriginID
	ORDER BY
		bt.DateAdded DESC
END
GO
