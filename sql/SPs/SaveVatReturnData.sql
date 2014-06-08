IF OBJECT_ID('SaveVatReturnData') IS NULL
	EXECUTE('CREATE PROCEDURE SaveVatReturnData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveVatReturnData
@CustomerMarketplaceID INT,
@HistoryRecordID INT,
@SourceID INT,
@Now DATETIME,
@VatReturnRecords VatReturnRawRecordList READONLY,
@VatReturnEntries VatReturnRawEntryList READONLY,
@RtiTaxMonthRawData RtiTaxMonthRawList READONLY,
@HistoryItems VatReturnHistoryList READONLY,
@OldDeletedItems IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRAN

	DECLARE @RtiID INT = 0
	DECLARE @RtiCount INT = 0
	DECLARE @DebugMode BIT = 0

	------------------------------------------------------------------------------
	--
	-- Detect debug mode.
	--
	------------------------------------------------------------------------------

	SELECT
		@DebugMode = (CASE LOWER(Value)
			WHEN '1' THEN 1
			WHEN 'yes' THEN 1
			WHEN 'true' THEN 1
			ELSE 0
		END)
	FROM
		ConfigurationVariables
	WHERE
		Name = 'DEBUG_SaveVatReturnData'

	IF @DebugMode IS NULL
		SET @DebugMode = 0

	------------------------------------------------------------------------------
	--
	-- Copy VAT return records from parameter to temp table while assigning
	-- corresponding business ID (if it exists).
	--
	------------------------------------------------------------------------------

	SELECT
		r.DateFrom,
		r.DateTo,
		r.DateDue,
		r.Period,
		r.RegistrationNo,
		r.BusinessName AS Name,
		r.Address,
		b.Id AS BusinessID,
		r.RecordID,
		r.SourceID,
		r.IsDeleted,
		r.InternalID
	INTO
		#VatReturnRecords
	FROM
		@VatReturnRecords r
		LEFT JOIN Business b
			ON r.BusinessName = b.Name
			AND r.Address = b.Address
			AND r.RegistrationNo = b.RegistrationNo

	------------------------------------------------------------------------------
	--
	-- Create missing businesses.
	--
	------------------------------------------------------------------------------

	INSERT INTO Business (Name, Address, RegistrationNo)
	SELECT DISTINCT
		Name,
		Address,
		RegistrationNo
	FROM
		#VatReturnRecords
	WHERE
		BusinessID IS NULL

	------------------------------------------------------------------------------
	--
	-- Fill business ID for newly created businesses.
	--
	------------------------------------------------------------------------------

	UPDATE #VatReturnRecords SET
		BusinessID = b.Id
	FROM
		#VatReturnRecords r
		INNER JOIN Business b
			ON r.Name = b.Name
			AND r.Address = b.Address
			AND r.RegistrationNo = b.RegistrationNo
			AND r.BusinessID IS NULL

	------------------------------------------------------------------------------
	--
	-- Update existing VAT return records.
	--
	------------------------------------------------------------------------------

	UPDATE MP_VatReturnRecords SET
		CustomerMarketPlaceId = @CustomerMarketplaceID,
		Created = @Now,
		CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryRecordID,
		Period = tmp.Period,
		DateFrom = tmp.DateFrom,
		DateTo = tmp.DateTo,
		DateDue = tmp.DateDue,
		BusinessId = tmp.BusinessID,
		RegistrationNo = tmp.RegistrationNo,
		IsDeleted = tmp.IsDeleted,
		SourceID = @SourceID,
		InternalID = tmp.InternalID
	FROM
		MP_VatReturnRecords r
		INNER JOIN #VatReturnRecords tmp
			ON r.Id = tmp.RecordID
			AND tmp.RecordID IS NOT NULL
			AND tmp.RecordID != 0

	------------------------------------------------------------------------------
	--
	-- Create new VAT return records.
	--
	------------------------------------------------------------------------------

	INSERT INTO MP_VatReturnRecords(
		CustomerMarketPlaceId,
		Created,
		CustomerMarketPlaceUpdatingHistoryRecordId,
		Period,
		DateFrom,
		DateTo,
		DateDue,
		BusinessId,
		RegistrationNo,
		IsDeleted,
		SourceID,
		InternalID
	) SELECT
		@CustomerMarketplaceID,
		@Now,
		@HistoryRecordID,
		tmp.Period,
		tmp.DateFrom,
		tmp.DateTo,
		tmp.DateDue,
		tmp.BusinessID,
		tmp.RegistrationNo,
		tmp.IsDeleted,
		@SourceID,
		tmp.InternalID
	FROM
		#VatReturnRecords tmp
	WHERE
		tmp.RecordID IS NULL
		OR
		tmp.RecordID = 0

	------------------------------------------------------------------------------
	--
	-- Debug output.
	--
	------------------------------------------------------------------------------

	IF @DebugMode = 1
	BEGIN
		SELECT 'VAT return records without RecordID in new records.' AS DebugMessage

		SELECT * FROM #VatReturnRecords
	END

	------------------------------------------------------------------------------
	--
	-- Fill record ID for new records.
	--
	------------------------------------------------------------------------------

	UPDATE #VatReturnRecords SET
		RecordID = r.Id
	FROM
		#VatReturnRecords tmp
		INNER JOIN MP_VatReturnRecords r
			ON tmp.InternalID = r.InternalID
	WHERE
		tmp.RecordID IS NULL
		OR
		tmp.RecordID = 0

	------------------------------------------------------------------------------
	--
	-- Debug output.
	--
	------------------------------------------------------------------------------

	IF @DebugMode = 1
	BEGIN
		SELECT 'VAT return records with RecordID in new records.' AS DebugMessage

		SELECT * FROM #VatReturnRecords
	END

	------------------------------------------------------------------------------
	--
	-- At this point VAT return records themselves are stored.
	--
	-- Copy VAT return entries from parameter to temp table while assigning
	-- corresponding box ID (if it exists).
	--
	------------------------------------------------------------------------------

	SELECT
		e.RecordInternalID,
		e.BoxName,
		n.Id AS BoxID,
		e.Amount,
		e.CurrencyCode
	INTO
		#VatReturnEntries
	FROM
		@VatReturnEntries e
		LEFT JOIN MP_VatReturnEntryNames n
			ON e.BoxName = n.Name

	------------------------------------------------------------------------------
	--
	-- Create missing box names.
	--
	------------------------------------------------------------------------------

	INSERT INTO MP_VatReturnEntryNames(Name)
	SELECT DISTINCT
		BoxName
	FROM
		#VatReturnEntries
	WHERE
		BoxID IS NULL

	------------------------------------------------------------------------------
	--
	-- Fill box ID for newly created boxes.
	--
	------------------------------------------------------------------------------

	UPDATE #VatReturnEntries SET
		BoxID = n.Id
	FROM
		#VatReturnEntries e
		INNER JOIN MP_VatReturnEntryNames n
			ON e.BoxName = n.Name
			AND e.BoxID IS NULL

	------------------------------------------------------------------------------
	--
	-- Mark old entries as deleted.
	--
	------------------------------------------------------------------------------

	UPDATE MP_VatReturnEntries SET
		IsDeleted = 1
	FROM
		MP_VatReturnEntries e
		INNER JOIN #VatReturnRecords r
			ON e.RecordId = r.RecordID

	------------------------------------------------------------------------------
	--
	-- Insert new entries.
	--
	------------------------------------------------------------------------------

	INSERT INTO MP_VatReturnEntries (RecordId, NameId, Amount, CurrencyCode, IsDeleted)
	SELECT
		r.RecordID,
		e.BoxID,
		e.Amount,
		e.CurrencyCode,
		0
	FROM
		#VatReturnEntries e
		INNER JOIN #VatReturnRecords r
			ON e.RecordInternalID = r.InternalID

	------------------------------------------------------------------------------
	--
	-- Debug output.
	--
	------------------------------------------------------------------------------

	IF @DebugMode = 1
	BEGIN
		SELECT 'VAT return entries.' AS DebugMessage

		SELECT * FROM #VatReturnEntries
	END

	------------------------------------------------------------------------------
	--
	-- At this point both entries and records are stored.
	--
	-- Checking how many RTI tax months received.
	--
	------------------------------------------------------------------------------

	SELECT
		@RtiCount = COUNT(*)
	FROM
		@RtiTaxMonthRawData

	------------------------------------------------------------------------------
	--
	-- Saving RTI tax months data.
	--
	------------------------------------------------------------------------------

	IF @RtiCount IS NOT NULL AND @RtiCount > 0
	BEGIN
		INSERT INTO MP_RtiTaxMonthRecords(
			CustomerMarketPlaceId,
			Created,
			CustomerMarketPlaceUpdatingHistoryRecordId,
			SourceID
		) VALUES (
			@CustomerMarketplaceID,
			@Now,
			@HistoryRecordID,
			@SourceID
		)

		SET @RtiID = SCOPE_IDENTITY()

		INSERT INTO MP_RtiTaxMonthEntries (
			DateStart,
			DateEnd,
			AmountPaid,
			AmountDue,
			CurrencyCode,
			RecordId
		)
		SELECT
			DateStart,
			DateEnd,
			PaidAmount,
			DueAmount,
			PaidCurrencyCode,
			@RtiID
		FROM
			@RtiTaxMonthRawData
	END

	------------------------------------------------------------------------------
	--
	-- Debug output.
	--
	------------------------------------------------------------------------------

	IF @DebugMode = 1
		SELECT 'RTI tax mont entries count is ' + CONVERT(NVARCHAR, @RtiCount) + ', record ID is ' + CONVERT(NVARCHAR, @RtiID) AS DebugMessage

	------------------------------------------------------------------------------
	--
	-- At this point VAT return data and RTI tax months data are saved.
	--
	-- Saving history.
	--
	------------------------------------------------------------------------------

	INSERT INTO MP_VatReturnRecordDeleteHistory(
		DeletedRecordID,
		ReasonRecordID,
		ReasonID,
		DeletedTime
	) SELECT
		r.Id,
		h.ReasonRecordID,
		h.ReasonID,
		@Now
	FROM
		@HistoryItems h
		INNER JOIN MP_VatReturnRecords r
			ON h.DeleteRecordInternalID = r.InternalID

	------------------------------------------------------------------------------
	--
	-- Mark old records deleted.
	--
	------------------------------------------------------------------------------

	UPDATE MP_VatReturnRecords SET
		IsDeleted = 1
	FROM
		MP_VatReturnRecords r
		INNER JOIN @OldDeletedItems o
			ON r.Id = o.Value

	------------------------------------------------------------------------------

	DROP TABLE #VatReturnEntries
	DROP TABLE #VatReturnRecords

	COMMIT TRAN
END
GO
