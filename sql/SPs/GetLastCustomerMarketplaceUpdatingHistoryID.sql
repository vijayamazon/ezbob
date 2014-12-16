IF OBJECT_ID('GetLastCustomerMarketplaceUpdatingHistoryID') IS NULL
	EXECUTE('CREATE PROCEDURE GetLastCustomerMarketplaceUpdatingHistoryID AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetLastCustomerMarketplaceUpdatingHistoryID
@MpType NVARCHAR(32),
@MpID INT,
@HistoryID INT,
@LastHistoryID INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @LastHistoryID = @HistoryID

	IF @LastHistoryID IS NOT NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Detect last history ID. It must be saved in ***Aggregation table and it
	-- is not specified when executing backfill; thus backfill is not possible
	-- for marketplaces that have never been upated.
	--
	------------------------------------------------------------------------------

	SELECT TOP 1
		@LastHistoryID = h.Id
	FROM
		MP_CustomerMarketPlaceUpdatingHistory h
	WHERE
		h.CustomerMarketPlaceId = @MpID
	ORDER BY
		h.UpdatingEnd DESC,
		h.Id DESC

	------------------------------------------------------------------------------

	IF @LastHistoryID IS NULL
		RAISERROR('No last updating history id found for %s marketplace with id %d.', 11, 1, @MpType, @MpID)
END
GO
