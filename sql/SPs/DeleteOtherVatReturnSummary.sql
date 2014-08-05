IF OBJECT_ID('DeleteOtherVatReturnSummary') IS NULL
	EXECUTE('CREATE PROCEDURE DeleteOtherVatReturnSummary AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE DeleteOtherVatReturnSummary
@CustomerID INT,
@CustomerMarketplaceID INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE MP_VatReturnSummary SET
		IsActive = 0
	WHERE
		CustomerID = @CustomerID
		AND
		CustomerMarketplaceID = @CustomerMarketplaceID
		AND
		ISActive = 1
END
GO
