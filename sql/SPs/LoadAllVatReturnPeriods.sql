IF OBJECT_ID('LoadAllVatReturnPeriods') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAllVatReturnPeriods AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAllVatReturnPeriods
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		r.Id,
		r.DateFrom,
		r.DateTo,
		r.RegistrationNo,
		r.SourceID
	FROM
		MP_VatReturnRecords r
		INNER JOIN MP_CustomerMarketPlace m
			ON r.CustomerMarketPlaceId = m.Id
		INNER JOIN Customer c
			ON m.CustomerId = c.Id
			AND c.Id = @CustomerID
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
	WHERE
		ISNULL(r.IsDeleted, 0) = 0
	ORDER BY
		r.Id
END
GO
