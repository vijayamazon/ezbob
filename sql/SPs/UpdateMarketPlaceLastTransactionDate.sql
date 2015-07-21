IF OBJECT_ID('UpdateMarketPlaceLastTransactionDate') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMarketPlaceLastTransactionDate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMarketPlaceLastTransactionDate
@MpID INT,
@LastTransactionDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE MP_CustomerMarketPlace SET LastTransactionDate = @LastTransactionDate
	WHERE Id=@MpID
END 
GO