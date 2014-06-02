IF OBJECT_ID('dbo.udfLoadRtiMonthRawData') IS NOT NULL
	DROP FUNCTION dbo.udfLoadRtiMonthRawData
GO

CREATE FUNCTION dbo.udfLoadRtiMonthRawData(@CustomerMarketplaceID INT)
RETURNS @out TABLE(
	RecordID INT,
	DateStart DATETIME,
	DateEnd DATETIME,
	AmountPaid DECIMAL(18, 2),
	AmountDue DECIMAL(18, 2),
	CurrencyCode NVARCHAR(3)
)
AS
BEGIN
	DECLARE @RecordID INT

	SELECT TOP 1
		@RecordID = Id
	FROM
		MP_RtiTaxMonthRecords
	WHERE
		CustomerMarketPlaceId = @CustomerMarketplaceID
	ORDER BY
		Created DESC

	INSERT INTO @out(RecordID, DateStart, DateEnd, AmountPaid, AmountDue, CurrencyCode)
	SELECT
		e.RecordId,
		e.DateStart,
		e.DateEnd,
		e.AmountPaid,
		e.AmountDue,
		e.CurrencyCode
	FROM
		MP_RtiTaxMonthEntries e
	WHERE
		e.RecordId = @RecordID

	RETURN
END
GO
