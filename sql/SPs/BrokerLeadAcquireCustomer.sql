IF OBJECT_ID('BrokerLeadAcquireCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadAcquireCustomer AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadAcquireCustomer
@CustomerID INT,
@LeadID INT
AS
BEGIN
	DECLARE @BrokerID INT

	SELECT
		@BrokerID = BrokerID
	FROM
		BrokerLeads
	WHERE
		BrokerLeadID = @LeadID
		AND
		CustomerID IS NULL

	IF @BrokerID IS NOT NULL AND NOT EXISTS (SELECT * FROM Customer WHERE Id = @CustomerID AND BrokerID IS NOT NULL)
	BEGIN
		BEGIN TRANSACTION
		
		UPDATE Customer SET BrokerID = @BrokerID WHERE Id = @CustomerID
		UPDATE BrokerLeads SET CustomerID = @CustomerID WHERE BrokerLeadID = @LeadID

		COMMIT TRANSACTION
	END
END
GO
