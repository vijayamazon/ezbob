IF OBJECT_ID('UpdateMpTotalsEbay') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsEbay AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsEbay
@MpID INT,
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------
	--
	-- Find last history id (for backfills).
	--
	------------------------------------------------------------------------------

	DECLARE @LastHistoryID INT

	EXECUTE GetLastCustomerMarketplaceUpdatingHistoryID 'eBay', @MpID, @HistoryID, @LastHistoryID OUTPUT

	IF @LastHistoryID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------


	SELECT
		@TerapeakSum = SUM(ISNULL(i.Revenue, 0)),
		@TerapeakDayCount = SUM(ISNULL(DATEDIFF(day, ld.StartDate, ld.EndDate), 0) + 1),
		@TerapeakFrom = MIN(ld.StartDate),
		@TerapeakTo = MAX(ld.EndDate)
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN #latest_data ld ON i.Id = ld.Id


END
GO
