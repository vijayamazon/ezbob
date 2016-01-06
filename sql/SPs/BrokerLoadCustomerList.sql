IF OBJECT_ID('BrokerLoadCustomerList') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCustomerList AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE BrokerLoadCustomerList
@ContactEmail NVARCHAR(255),
@Origin INT,
@BrokerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @BrokerOriginID INT = NULL

	------------------------------------------------------------------------------

	IF @BrokerID IS NULL OR @BrokerID <= 0
	BEGIN
		SELECT
			@BrokerID = BrokerID,
			@BrokerOriginID = OriginID
		FROM
			Broker
		WHERE
			ContactEmail = @ContactEmail
			AND
			OriginID = @Origin
	END
	ELSE BEGIN
		SELECT
			@BrokerOriginID = OriginID
		FROM
			Broker
		WHERE
			BrokerID = @BrokerID
	END

	------------------------------------------------------------------------------

	;WITH last_cashrquests AS (
		SELECT 
			MAX(cr.Id) maxid  
		FROM 
			CashRequests cr 
			INNER JOIN Customer c
				ON cr.IdCustomer = c.Id
				AND c.BrokerID = @BrokerID	
		GROUP BY
			cr.IdCustomer
	), last_cashrquests_prepare AS (
		SELECT 
			cr.Id,
			cr.IdCustomer,
			cr.ManagerApprovedSum
		FROM 
			CashRequests cr 
			INNER JOIN last_cashrquests lcr ON lcr.maxid = cr.Id
		WHERE
			cr.UnderwriterDecision = 'Approved'	
	) SELECT
		c.Id AS CustomerID,
		c.FirstName AS FirstName,
		c.Surname AS LastName,
		c.Name AS Email,
		c.IsWaitingForSignature AS Signature,
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
		dbo.udfGetMpsTypes(c.Id) AS Marketplaces,
		ISNULL(l.LoanAmount, 0) AS LoanAmount,
		l.Date AS LoanDate,
		l.SetupFee,
		ISNULL(lcp.ManagerApprovedSum, 0) AS ApprovedAmount,
		ISNULL(lb.CommissionAmount, 0) AS CommissionAmount, 
		lb.PaidDate AS CommissionPaymentDate
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
		LEFT JOIN Loan l ON l.CustomerId = c.Id AND l.Position = 0
		LEFT JOIN last_cashrquests_prepare lcp ON lcp.IdCustomer = c.Id
		LEFT JOIN LoanBrokerCommission lb ON lb.LoanID = l.Id
	WHERE
		c.BrokerID = @BrokerID
		AND
		c.OriginID = @BrokerOriginID
	ORDER BY
		c.Id

	------------------------------------------------------------------------------
END

GO


