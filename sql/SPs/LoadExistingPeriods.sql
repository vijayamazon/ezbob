IF OBJECT_ID('LoadExistingPeriods') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExistingPeriods AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadExistingPeriods
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MarketplaceID INT

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

	SELECT
		MAX(r.Id) AS RecordID
	INTO
		#recs
	FROM
		MP_VatReturnRecords r
	WHERE
		r.CustomerMarketPlaceId = @MarketplaceID
		AND
		ISNULL(r.IsDeleted, 0) = 0
	GROUP BY
		CONVERT(DATE, r.DateFrom),
		CONVERT(DATE, r.DateTo),
		r.RegistrationNo

	SELECT
		CONVERT(DATE, r.DateFrom) AS DateFrom,
		CONVERT(DATE, r.DateTo) AS DateTo,
		r.RegistrationNo,
		b.Name
	FROM
		MP_VatReturnRecords r
		INNER JOIN #recs ON r.Id = #recs.RecordID
		INNER JOIN Business b ON r.BusinessId = b.Id
	ORDER BY
		r.RegistrationNo,
		r.DateFrom,
		b.Name

	DROP TABLE #recs
END
GO
