IF OBJECT_ID('BrokerLoadCustomerDetails') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCustomerDetails AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCustomerDetails
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255),
@Origin INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT

	SELECT
		@BrokerID = BrokerID
	FROM
		Broker
	WHERE
		ContactEmail = @ContactEmail
		AND
		OriginID = @Origin

	SELECT
		c.Id AS CustomerID,
		c.FirstName,
		c.Surname,
		c.DateOfBirth,
		c.Gender,
		c.Name AS Email,
		c.MaritalStatus,
		c.MobilePhone,
		c.DaytimePhone,
		a.Organisation,
		a.Line1,
		a.Line2,
		a.Line3,
		a.Pobox,
		a.Town,
		a.County,
		a.Postcode,
		a.Country,
		w.TheLastOne FinishedWizard,
		b.BrokerLeadID LeadID
	FROM
		Customer c
		LEFT JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType = 1
		LEFT JOIN WizardStepTypes w ON w.WizardStepTypeID = c.WizardStep
		LEFT JOIN BrokerLeads b ON b.CustomerID = c.Id
	WHERE
		c.RefNumber = @RefNum
		AND
		c.BrokerID = @BrokerID
END
GO
