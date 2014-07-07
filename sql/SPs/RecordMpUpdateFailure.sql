IF OBJECT_ID('RecordMpUpdateFailure') IS NULL
	EXECUTE('CREATE PROCEDURE RecordMpUpdateFailure AS SELECT 1')
GO

ALTER PROCEDURE RecordMpUpdateFailure
@MpId INT
AS
BEGIN
	DECLARE 
		@Error NVARCHAR(MAX),
		@EndTime DATETIME
		
	SELECT @Error = UpdateError, @EndTime = UpdatingEnd FROM MP_CustomerMarketPlace WHERE Id = @MpId
	
	IF @Error IS NOT NULL AND @Error != ''
	BEGIN
		UPDATE MP_CustomerMarketPlace SET UpdateError = 'Strategy failed' WHERE Id = @MpId
	END
	
	IF @EndTime IS NULL
	BEGIN
		UPDATE MP_CustomerMarketPlace SET UpdatingEnd = GETUTCDATE() WHERE Id = @MpId
	END
END
GO
