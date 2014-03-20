IF OBJECT_ID('BrokerLeadLoadBroker') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadLoadBroker AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadLoadBroker
@LeadID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT
	
	SELECT
		@BrokerID = BrokerID
	FROM
		BrokerLeads
	WHERE
		BrokerLeadID = @LeadID
		AND
		DateDeleted IS NULL
		AND
		BrokerLeadDeletedReasonID IS NULL

	EXECUTE BrokerLoadContactData @BrokerID
END
GO
