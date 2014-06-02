IF OBJECT_ID('LoadDataForVatReturnSummary') IS NULL
	EXECUTE('CREATE PROCEDURE LoadDataForVatReturnSummary AS SELECT 1')
GO

ALTER PROCEDURE LoadDataForVatReturnSummary
@CustomerMarketplaceID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		e.Amount,
		e.CurrencyCode,
		en.Name AS BoxName,
		r.DateFrom,
		r.DateTo,
		r.BusinessId AS BusinessID
	FROM
		MP_VatReturnEntries e
		INNER JOIN dbo.udfLoadActualVatReturnRecords(@CustomerMarketPlaceID) re ON e.RecordId = re.RecordID
		INNER JOIN MP_VatReturnRecords r ON re.RecordID = r.Id
		INNER JOIN MP_VatReturnEntryNames en ON e.NameId = en.Id
	ORDER BY
		r.BusinessId,
		r.DateFrom
END
GO
