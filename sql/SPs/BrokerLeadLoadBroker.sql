IF OBJECT_ID('BrokerLeadLoadBroker') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadLoadBroker AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadLoadBroker
@LeadID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		b.BrokerID,
		b.ContactEmail AS BrokerContactEmail,
		c.Id AS CustomerID,
		w.TheLastOne AS IsAtLastWizardStep
	FROM
		BrokerLeads bl
		INNER JOIN Broker b ON bl.BrokerID = b.BrokerID
		INNER JOIN Customer c ON bl.CustomerID = c.Id
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	WHERE
		bl.BrokerLeadID = @LeadID
END
GO
