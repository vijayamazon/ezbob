SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('VatReturnRawEntryList') IS NULL
BEGIN
	CREATE TYPE VatReturnRawEntryList AS TABLE (
		RecordInternalID UNIQUEIDENTIFIER,
		BoxName NVARCHAR(512),
		Amount DECIMAL(18, 2),
		CurrencyCode NVARCHAR(3)
	)
END
GO
