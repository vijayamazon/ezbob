IF OBJECT_ID('BrokerLoadCustomerList') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCustomerList AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCustomerList
@ContactEmail NVARCHAR(255),
@BrokerID INT
AS
BEGIN
	SET NOCOUNT ON;

	IF @BrokerID IS NULL OR @BrokerID <= 0
		SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail

	SELECT
		c.Id AS CustomerID,
		c.FirstName AS FirstName,
		c.Surname AS LastName,
		c.Name AS Email,
		c.RefNumber,
		w.WizardStepTypeDescription AS WizardStep,
		CASE
			WHEN W.WizardStepTypeName = 'success' THEN
				CASE
					WHEN C.LastLoanDate IS NOT NULL THEN 'Loan'
				ELSE
					CASE
						WHEN C.NumApproves > 0 THEN 'Approved'
						ELSE ISNULL(c.CreditResult, 'In process')
					END
				END
			WHEN W.WizardStepTypeName = 'link' THEN 'Link Accounts'
			WHEN W.WizardStepTypeName = 'companydetails' THEN 'Company Details'
			WHEN W.WizardStepTypeName = 'details' THEN 'Personal Details'
			WHEN W.WizardStepTypeName = 'signup' THEN 'Application Step 1'
			ELSE 'Unknown: ' + W.WizardStepTypeName
		END AS Status,
		c.GreetingMailSentDate AS ApplyDate,
		ISNULL(t.Name, '') AS MpTypeName,
		ISNULL(l.LoanAmount, 0) AS LoanAmount,
		l.Date AS LoanDate,
		l.SetupFee
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
		LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
		LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
		LEFT JOIN Loan l ON l.CustomerId = c.Id AND l.Position = 0
	WHERE
		c.BrokerID = @BrokerID
	ORDER BY
		c.Id
END
GO
