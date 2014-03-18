IF OBJECT_ID('BrokerLeadLoadBroker') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadLoadBroker AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadLoadBroker
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

	EXECUTE BrokerLoadContactData @BrokerID
END
GO
