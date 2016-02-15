SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_GetLastCashRequestExpired') IS NULL
	EXECUTE('CREATE PROCEDURE I_GetLastCashRequestExpired AS SELECT 1')
GO

ALTER PROCEDURE I_GetLastCashRequestExpired
	@CustomerID INT 
AS
BEGIN

	DECLARE @LastCRID BIGINT = (SELECT max(Id) FROM CashRequests WHERE IdCustomer = @CustomerID)
	
	IF @LastCRID IS NULL
	BEGIN
		SELECT CAST(0 AS BIT) AS IsOpenPlatform, CAST(0 AS BIT) AS IsApproved 
	END
	ELSE
	BEGIN
		SELECT 
			cr.Id AS CashRequestID,
			c.CreditSum, 
			CAST(CASE WHEN cr.UnderwriterDecision = 'Approved' THEN 1 ELSE 0 END AS BIT) AS IsApproved,
			CAST(CASE WHEN p.FundingTypeID IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsOpenPlatform,
			o.InvestorID,
			o.InvestmentPercent,
			b.InvestorBankAccountID AS FundingBankAccountID
		FROM 
			Customer c
		LEFT JOIN 
			CashRequests cr ON c.Id = cr.IdCustomer
		LEFT JOIN 
			I_OpenPlatformOffer o ON o.CashRequestID = cr.Id	
		LEFT JOIN 
			I_ProductSubType p ON p.ProductSubTypeID = cr.ProductSubTypeID
		LEFT JOIN 
			I_InvestorBankAccount b ON b.InvestorID = o.InvestorID AND b.InvestorAccountTypeID=1 AND b.IsActive = 1
		WHERE 
			c.Id = @CustomerID
		AND 
			cr.Id = @LastCRID
	END
END
GO
