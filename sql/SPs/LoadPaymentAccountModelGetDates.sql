IF OBJECT_ID('LoadPaymentAccountModelGetDates') IS NULL
	EXECUTE('CREATE PROCEDURE LoadPaymentAccountModelGetDates AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadPaymentAccountModelGetDates
@MpID INT,
@Now DATETIME,
@ShowCurrent BIT OUTPUT,
@CurrentMonthEnd DATETIME OUTPUT,
@CurrentMonthStart DATETIME OUTPUT,
@YearAgo DATETIME OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SET @ShowCurrent = ISNULL(@ShowCurrent, 1)

	------------------------------------------------------------------------------

	SET @CurrentMonthEnd = dbo.udfMonthEnd(dbo.udfGetLatestTotalsMonth(@MpID, @Now))

	------------------------------------------------------------------------------

	SET @CurrentMonthStart = dbo.udfMonthStart(@CurrentMonthEnd)

	------------------------------------------------------------------------------

	SET @YearAgo = dbo.udfMonthStart(DATEADD(month, -11, @CurrentMonthEnd))	
END
GO
