SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('RtiTaxMonthRawList') IS NULL
BEGIN
	CREATE TYPE RtiTaxMonthRawList AS TABLE (
		DateStart DATETIME,
		DateEnd DATETIME,
		PaidAmount DECIMAL(18, 2),
		PaidCurrencyCode NVARCHAR(3),
		DueAmount DECIMAL(18, 2),
		DueCurrencyCode NVARCHAR(3)
	)
END
GO
