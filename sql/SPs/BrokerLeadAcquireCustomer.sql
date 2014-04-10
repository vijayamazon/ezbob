IF OBJECT_ID('BrokerLeadAcquireCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadAcquireCustomer AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadAcquireCustomer
@CustomerID INT,
@LeadID INT,
@BrokerFillsForCustomer BIT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT
	DECLARE @IsTest BIT

	SELECT
		@BrokerID = BrokerID
	FROM
		BrokerLeads
	WHERE
		BrokerLeadID = @LeadID
		AND
		CustomerID IS NULL
		AND
		DateDeleted IS NULL
		AND
		BrokerLeadDeletedReasonID IS NULL

	IF @BrokerID IS NOT NULL AND NOT EXISTS (SELECT * FROM Customer WHERE Id = @CustomerID AND BrokerID IS NOT NULL)
	BEGIN
		SELECT
			@IsTest = IsTest
		FROM
			Broker
		WHERE
			BrokerID = @BrokerID

		BEGIN TRANSACTION
		
		UPDATE Customer SET
			BrokerID = @BrokerID,
			FilledByBroker = @BrokerFillsForCustomer,
			IsTest = CASE @IsTest WHEN 1 THEN 1 ELSE IsTest END
		WHERE
			Id = @CustomerID

		UPDATE BrokerLeads SET
			CustomerID = @CustomerID
		WHERE
			BrokerLeadID = @LeadID

		COMMIT TRANSACTION
	END
END
GO
