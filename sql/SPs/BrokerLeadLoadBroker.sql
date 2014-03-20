IF OBJECT_ID('BrokerLeadLoadBroker') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadLoadBroker AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadLoadBroker
@LeadID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT
	DECLARE @CustomerID INT
	
	SELECT
		@BrokerID = BrokerID,
		@CustomerID = CustomerID
	FROM
		BrokerLeads
	WHERE
		BrokerLeadID = @LeadID

	SELECT
		b.BrokerID,
		b.ContactEmail AS BrokerContactEmail,
		@CustomerID AS CustomerID
	FROM
		Broker b
	WHERE
		b.BrokerID = @BrokerID
END
GO
