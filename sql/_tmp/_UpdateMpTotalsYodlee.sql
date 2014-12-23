IF OBJECT_ID('UpdateMpTotalsYodlee') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsYodlee AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsYodlee
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

	EXECUTE GetLastCustomerMarketplaceUpdatingHistoryID 'Yodlee', @MpID, @HistoryID, @LastHistoryID OUTPUT

	IF @LastHistoryID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

END
GO
