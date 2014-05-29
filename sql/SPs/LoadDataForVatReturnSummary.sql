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
		MAX(r.Id) AS RecordID
	INTO
		#recs
	FROM
		MP_VatReturnRecords r
	WHERE
		r.CustomerMarketPlaceId = @CustomerMarketplaceID
		AND
		ISNULL(r.IsDeleted, 0) = 0
	GROUP BY
		CONVERT(DATE, r.DateFrom),
		CONVERT(DATE, r.DateTo),
		r.RegistrationNo

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
		INNER JOIN #recs ON e.RecordId = #recs.RecordID
		INNER JOIN MP_VatReturnRecords r ON #recs.RecordID = r.Id
		INNER JOIN MP_VatReturnEntryNames en ON e.NameId = en.Id
	ORDER BY
		r.BusinessId,
		r.DateFrom

	------------------------------------------------------------------------------

	DROP TABLE #recs
END
GO
