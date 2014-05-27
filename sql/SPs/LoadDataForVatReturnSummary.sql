IF OBJECT_ID('LoadDataForVatReturnSummary') IS NULL
	EXECUTE('CREATE PROCEDURE LoadDataForVatReturnSummary AS SELECT 1')
GO

ALTER PROCEDURE LoadDataForVatReturnSummary
@CustomerMarketplaceID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	IF OBJECT_ID('#recs') IS NOT NULL
		DROP TABLE #recs

	------------------------------------------------------------------------------

	SELECT
		MAX(r.Id) AS RecordID,
		r.Period,
		CONVERT(DATE, r.DateFrom) AS DateFrom,
		CONVERT(DATE, r.DateTo) AS DateTo,
		r.RegistrationNo,
		r.BusinessId
	INTO
		#recs
	FROM
		MP_VatReturnRecords r
	WHERE
		r.CustomerMarketPlaceId = @CustomerMarketplaceID
		AND
		ISNULL(r.IsDeleted, 0) = 0
	GROUP BY
		r.Period,
		CONVERT(DATE, r.DateFrom),
		CONVERT(DATE, r.DateTo),
		r.RegistrationNo,
		r.BusinessId

	------------------------------------------------------------------------------

	SELECT
		e.Amount,
		e.CurrencyCode,
		en.Name AS BoxName,
		r.DateFrom,
		r.DateTo,
		r.BusinessId AS BusinessID
	FROM
		MP_VatReturnEntries e
		INNER JOIN #recs r ON e.RecordId = r.RecordID
		INNER JOIN MP_VatReturnEntryNames en ON e.NameId = en.Id
	ORDER BY
		r.BusinessId,
		r.DateFrom

	------------------------------------------------------------------------------

	DROP TABLE #recs
END
GO
