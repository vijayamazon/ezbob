IF OBJECT_ID('UpdateMarketplaceSecurityData') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMarketplaceSecurityData AS SELECT 1')
GO

ALTER PROCEDURE UpdateMarketplaceSecurityData
@CustomerMarketplaceID INT,
@SecurityData VARBINARY(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE MP_CustomerMarketPlace SET
		SecurityData = @SecurityData
	WHERE
		Id = @CustomerMarketplaceID
END
GO
