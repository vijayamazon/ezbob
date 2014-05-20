IF OBJECT_ID('ApplyEncryptedMarketplaceSecurityData') IS NULL
	EXECUTE('CREATE PROCEDURE ApplyEncryptedMarketplaceSecurityData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE ApplyEncryptedMarketplaceSecurityData
@BatchID UNIQUEIDENTIFIER,
@DoRollback BIT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE MP_CustomerMarketPlace SET
		SecurityData = CASE @DoRollback WHEN 1 THEN OldData ELSE NewData END
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN TMP_CustomerMarketplaces t
			ON m.Id = t.MarketplaceID
			AND t.BatchID = @BatchID
END
GO
