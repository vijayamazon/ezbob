IF OBJECT_ID('BrokerLeadLoadDataForEmail') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadLoadDataForEmail AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadLoadDataForEmail
@LeadID INT,
@ContactEmail NVARCHAR(255)
AS
BEGIN
	DECLARE @BrokerID INT

	SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail

	SELECT
		bl.BrokerLeadID AS LeadID,
		bl.FirstName,
		bl.LastName,
		bl.Email
	FROM
		BrokerLeads bl
	WHERE
		bl.BrokerLeadID = @LeadID
		AND 
		bl.BrokerID = @BrokerID
END
GO
