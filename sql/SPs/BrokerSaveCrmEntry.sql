IF OBJECT_ID('BrokerSaveCrmEntry') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerSaveCrmEntry AS SELECT 1')
GO

ALTER PROCEDURE BrokerSaveCrmEntry
@IsIncoming BIT,
@ActionID INT,
@StatusID INT,
@Comment VARCHAR(1000),
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255),
@EntryTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrorMsg AS NVARCHAR(1024) = ''
	DECLARE @BrokerID INT
	DECLARE @BrokerName NVARCHAR(255)
	DECLARE @CustomerID INT

	IF @ErrorMsg = ''
	BEGIN
		SELECT
			@BrokerID = BrokerID,
			@BrokerName = FirmName
		FROM
			Broker
		WHERE
			ContactEmail = @ContactEmail
	
		IF @BrokerID IS NULL
			SET @ErrorMsg = 'Could not find broker by contact email ' + @ContactEmail
	END

	IF @ErrorMsg = ''
	BEGIN
		SELECT @CustomerID = Id FROM Customer WHERE RefNumber = @RefNum AND BrokerID = @BrokerID

		IF @CustomerID IS NULL
			SET @ErrorMsg = 'The broker is not authorised to access this customer data.'
	END

	IF @ErrorMsg = ''
	BEGIN
		BEGIN TRY
			INSERT INTO CustomerRelations(CustomerId, UserName, Incoming, ActionId, StatusId, Comment, Timestamp)
				VALUES (@CustomerID, @BrokerName, @IsIncoming, @ActionID, @StatusID, @Comment, @EntryTime)
		END TRY
		BEGIN CATCH
			SET @ErrorMsg = 'Failed to insert new CRM entry into database.'
		END CATCH
	END
	
	SELECT @ErrorMsg AS ErrorMsg
END
GO
