IF OBJECT_ID('GetMarketplaceFromHistoryID') IS NULL
	EXECUTE('CREATE PROCEDURE GetMarketplaceFromHistoryID AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMarketplaceFromHistoryID
@MpType NVARCHAR(32),
@HistoryID INT,
@MpID INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SET @MpID = NULL

	------------------------------------------------------------------------------

	IF @HistoryID IS NULL
	BEGIN
		RAISERROR('No updating history id specified for %s marketplace.', 11, 1, @MpType)
		RETURN
	END

	------------------------------------------------------------------------------

	SELECT
		@MpID = h.CustomerMarketplaceId
	FROM
		MP_CustomerMarketPlaceUpdatingHistory h
	WHERE
		h.Id = @HistoryID

	------------------------------------------------------------------------------

	IF @MpID IS NULL
	BEGIN
		RAISERROR('No marketplace id found for %s marketplace with history id %d.', 11, 2, @MpType, @HistoryID)
		RETURN
	END

	------------------------------------------------------------------------------
END
GO
