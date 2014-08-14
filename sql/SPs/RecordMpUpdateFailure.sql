IF OBJECT_ID('RecordMpUpdateFailure') IS NULL
	EXECUTE('CREATE PROCEDURE RecordMpUpdateFailure AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RecordMpUpdateFailure
@MpId INT,
@Error NVARCHAR(MAX),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE MP_CustomerMarketPlace SET
		UpdatingEnd = @Now,
		UpdateError = CASE
			WHEN @Error IS NOT NULL AND @Error != '' THEN @Error
			ELSE 'Strategy failed'
		END
	WHERE
		Id = @MpId
END
GO
