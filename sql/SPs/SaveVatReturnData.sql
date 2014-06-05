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
@VatREturnEntries VatReturnRawEntryList READONLY,
@RtiTaxMonthRawData RtiTaxMonthRawList READONLY,
@HistoryItems VatReturnHistoryList READONLY,
@OldDeletedItems IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;
END
GO
