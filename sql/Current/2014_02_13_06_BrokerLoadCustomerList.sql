IF OBJECT_ID('BrokerLoadCustomerList') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCustomerList AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCustomerList
@ContactEmail NVARCHAR(255)
AS
BEGIN
	DECLARE @BrokerID INT

	SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail

	SELECT
		c.Id AS CustomerID,
		c.FirstName AS FirstName,
		c.Surname AS LastName,
		c.Name AS Email,
		w.WizardStepTypeDescription AS WizardStep,
		ISNULL(c.CreditResult, 'In process') AS Status,
		c.ApplyForLoan AS ApplyDate,
		ISNULL(t.Name, '') AS MpTypeName,
		ISNULL(l.LoanAmount, 0) AS LoanAmount,
		l.Date AS LoanDate
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
		LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
		LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
		LEFT JOIN Loan l ON l.CustomerId = c.Id
	WHERE
		c.BrokerID = @BrokerID
	ORDER BY
		c.Id
END
GO
