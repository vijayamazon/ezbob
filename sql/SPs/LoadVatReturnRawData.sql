IF OBJECT_ID('LoadVatReturnRawData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadVatReturnRawData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadVatReturnRawData
@CustomerMarketplaceID INT
AS
BEGIN
	SET NOCOUNT ON

	IF OBJECT_ID('#recs') IS NOT NULL
		DROP TABLE #recs

	SELECT
		RecordID
	INTO
		#recs
	FROM
		dbo.udfLoadActualVatReturnRecords(@CustomerMarketplaceID)

	SELECT
		'record' AS RowType,
		r.Id AS RecordID,
		r.DateFrom,
		r.DateTo,
		r.DateDue,
		r.Period,
		r.RegistrationNo,
		b.Name AS BusinessName,
		b.Address,
		r.SourceID,
		r.InternalID
	FROM
		MP_VatReturnRecords r
		INNER JOIN #recs ON r.Id = #recs.RecordID
		INNER JOIN Business b ON r.BusinessId = b.Id

	SELECT
		'entry' AS RowType,
		e.RecordId AS RecordID,
		n.Name,
		e.Amount,
		e.CurrencyCode
	FROM
		MP_VatReturnEntries e
		INNER JOIN #recs ON e.RecordId = #recs.RecordID
		INNER JOIN MP_VatReturnEntryNames n ON e.NameId = n.Id

	SELECT
		'rti' AS RowType,
		DateStart,
		DateEnd,
		AmountPaid AS PaidAmount,
		CurrencyCode AS PaidCurrencyCode,
		AmountDue AS DueAmount,
		CurrencyCode AS DueCurrencyCode
	FROM
		dbo.udfLoadRtiMonthRawData(@CustomerMarketplaceID)

	DROP TABLE #recs
END
GO
