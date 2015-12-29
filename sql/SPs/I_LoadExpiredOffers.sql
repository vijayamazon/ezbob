IF OBJECT_ID('I_LoadExpiredOffers') IS NULL
	EXECUTE('CREATE PROCEDURE I_LoadExpiredOffers AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_LoadExpiredOffers
@Now DATETIME,
@LastCheckTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		cr.Id CashRequestID, 
		cr.IdCustomer CustomerID, 
		cr.ManagerApprovedSum, 
		c.CreditSum, 
		o.InvestorID, 
		o.InvestmentPercent, 
		ibFunding.InvestorBankAccountID
	FROM 
		CashRequests cr
	INNER JOIN 
		I_OpenPlatformOffer o ON o.CashRequestID = cr.Id
	INNER JOIN 
		Customer c ON c.Id = cr.IdCustomer
	INNER JOIN 
		I_InvestorBankAccount ibFunding 
		ON 
			o.InvestorID=ibFunding.InvestorID 
		AND 
			ibFunding.InvestorAccountTypeID=1 
		AND 
			ibFunding.IsActive=1
	WHERE 
			cr.OfferValidUntil > @LastCheckTime 
		AND
			cr.OfferValidUntil < @Now 
		AND 
			cr.UnderwriterDecision = 'Approved'
		AND 
			c.CreditSum > 0	
END 
GO
