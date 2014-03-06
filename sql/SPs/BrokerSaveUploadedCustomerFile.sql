IF OBJECT_ID('BrokerSaveUploadedCustomerFile') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerSaveUploadedCustomerFile AS SELECT 1')
GO

ALTER PROCEDURE BrokerSaveUploadedCustomerFile
@FileName NVARCHAR(500),
@CustomerID INT,
@ContactEmail NVARCHAR(255),
@FileContents VARBINARY(MAX),
@UploadedTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrorMsg NVARCHAR(1024) = ''
	DECLARE @UserID INT
	DECLARE @BrokerID INT

	IF @ErrorMsg = ''
	BEGIN
		SELECT
			@BrokerID = b.BrokerID,
			@UserID = b.UserID
		FROM
			Broker b
		WHERE
			b.ContactEmail = @ContactEmail

		IF @BrokerID IS NULL OR @UserID IS NULL
			SET @ErrorMsg = 'BrokerID/UserID not found by contact email ' + @ContactEmail
	END

	IF @ErrorMsg = ''
	BEGIN
		IF NOT EXISTS (SELECT Id FROM Customer WHERE Id = @CustomerID AND BrokerID = @BrokerID)
			SET @ErrorMsg = 'Customer ' + CONVERT(NVARCHAR, @CustomerID) + ' data is not accessible to broker ' + CONVERT(NVARCHAR, @BrokerID)
	END

	INSERT INTO MP_AlertDocument (DocName, UploadDate, UserId, CustomerId, Description, BinaryBody)
		VALUES (@FileName, @UploadedTime, @UserID, @CustomerID, @FileName, @FileContents)

	SELECT @ErrorMsg AS ErrorMsg
END
GO

