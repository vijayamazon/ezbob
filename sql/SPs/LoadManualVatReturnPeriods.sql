IF OBJECT_ID('LoadManualVatReturnPeriods') IS NULL
	EXECUTE('CREATE PROCEDURE LoadManualVatReturnPeriods AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadManualVatReturnPeriods
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MarketplaceID INT

	------------------------------------------------------------------------------

	SELECT
		@MarketplaceID = m.Id
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN Customer c
			ON m.CustomerId = c.Id
			AND m.DisplayName = c.Name
			AND c.Id = @CustomerID
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	------------------------------------------------------------------------------

	SELECT
		CONVERT(DATE, r.DateFrom) AS DateFrom,
		CONVERT(DATE, r.DateTo) AS DateTo,
		r.RegistrationNo,
		b.Name,
		r.SourceID,
		r.InternalID
	FROM
		MP_VatReturnRecords r
		INNER JOIN dbo.udfLoadActualVatReturnRecords(@MarketPlaceID) re ON r.Id = re.RecordID
		INNER JOIN Business b ON r.BusinessId = b.Id
	ORDER BY
		r.RegistrationNo,
		r.DateFrom,
		b.Name
END
GO
