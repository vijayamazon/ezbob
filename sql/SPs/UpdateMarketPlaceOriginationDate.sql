IF OBJECT_ID('UpdateMarketPlaceOriginationDate') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMarketPlaceOriginationDate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMarketPlaceOriginationDate
@MpID INT,
@OriginationDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE MP_CustomerMarketPlace SET OriginationDate = @OriginationDate
	WHERE Id=@MpID
END 
GO