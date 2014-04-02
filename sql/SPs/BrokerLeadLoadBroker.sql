IF OBJECT_ID('BrokerLeadLoadBroker') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadLoadBroker AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadLoadBroker
@LeadID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		ISNULL(b.BrokerID, 0) AS BrokerID,
		ISNULL(b.ContactEmail, '') AS BrokerContactEmail,
		ISNULL(c.Id, 0) AS CustomerID,
		ISNULL(w.TheLastOne, 0) AS IsAtLastWizardStep
	FROM
		BrokerLeads bl
		INNER JOIN Broker b ON bl.BrokerID = b.BrokerID
		LEFT JOIN Customer c ON bl.CustomerID = c.Id
		LEFT JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	WHERE
		bl.BrokerLeadID = @LeadID
END
GO
