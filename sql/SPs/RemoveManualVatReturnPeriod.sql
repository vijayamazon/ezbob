IF OBJECT_ID('RemoveManualVatReturnPeriod') IS NULL
	EXECUTE('CREATE PROCEDURE RemoveManualVatReturnPeriod AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RemoveManualVatReturnPeriod
@PeriodID UNIQUEIDENTIFIER,
@ReasonID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RecordID INT = 0

	------------------------------------------------------------------------------

	SELECT
		@RecordID = Id
	FROM
		MP_VatReturnRecords
	WHERE
		InternalID = @PeriodID

	------------------------------------------------------------------------------

	IF @RecordID IS NOT NULL AND @RecordID > 0
	BEGIN
		BEGIN TRAN

		-------------------------------------------------------------------------

		UPDATE MP_VatReturnRecords SET
			IsDeleted = 1
		WHERE
			Id = @RecordID

		-------------------------------------------------------------------------

		INSERT INTO MP_VatReturnRecordDeleteHistory(DeletedRecordID, ReasonID, DeletedTime)
			VALUES (@RecordID, @ReasonID, @Now)

		-------------------------------------------------------------------------

		COMMIT TRAN
	END

	------------------------------------------------------------------------------

	SELECT
		CustomerMarketPlaceID
	FROM
		MP_VatReturnRecords
	WHERE
		Id = @RecordID
END
GO
