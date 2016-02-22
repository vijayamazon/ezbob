IF OBJECT_ID('UpdateCustomerUploadedHmrcAccountData') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateCustomerUploadedHmrcAccountData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateCustomerUploadedHmrcAccountData
@MpID INT,
@DisplayName NVARCHAR(512),
@SecurityData VARBINARY(MAX)
AS
BEGIN
	UPDATE MP_CustomerMarketPlace SET
		DisplayName = @DisplayName,
		SecurityData = @SecurityData
	WHERE
		Id = @MpID
END
GO
