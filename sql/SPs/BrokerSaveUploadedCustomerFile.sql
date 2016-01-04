IF OBJECT_ID('BrokerSaveUploadedCustomerFile') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerSaveUploadedCustomerFile AS SELECT 1')
GO

ALTER PROCEDURE BrokerSaveUploadedCustomerFile
@FileName NVARCHAR(500),
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255),
@FileContents VARBINARY(MAX),
@UploadedTime DATETIME,
@Origin INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrorMsg NVARCHAR(1024) = ''
	DECLARE @BrokerID INT
	DECLARE @CustomerID INT

	IF @ErrorMsg = ''
	BEGIN
		SELECT
			@BrokerID = b.BrokerID
		FROM
			Broker b
		WHERE
			b.ContactEmail = @ContactEmail
			AND
			b.OriginID = @Origin

		IF @BrokerID IS NULL
			SET @ErrorMsg = 'BrokerID not found by contact email ' + @ContactEmail
	END

	IF @ErrorMsg = ''
	BEGIN
		SELECT @CustomerID = Id FROM Customer WHERE RefNumber = @RefNum AND BrokerID = @BrokerID

		IF @CustomerID IS NULL
			SET @ErrorMsg = 'Customer ' + @RefNum + ' data is not accessible to broker ' + CONVERT(NVARCHAR, @BrokerID)
	END

	INSERT INTO MP_AlertDocument (DocName, UploadDate, UserId, CustomerId, Description, BinaryBody)
		VALUES (@FileName, @UploadedTime, @BrokerID, @CustomerID, @FileName, @FileContents)

	SELECT @ErrorMsg AS ErrorMsg
END
GO

