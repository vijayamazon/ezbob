IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerLoadCustomerList]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BrokerLoadCustomerList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BrokerLoadCustomerList] 
	(@ContactEmail NVARCHAR(255))
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
		LEFT JOIN Loan l ON l.CustomerId = c.Id AND l.Position = 0
	WHERE
		c.BrokerID = @BrokerID
	ORDER BY
		c.Id
END
GO
