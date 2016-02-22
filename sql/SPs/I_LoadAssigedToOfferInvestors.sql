IF OBJECT_ID('I_LoadAssigedToOfferInvestors') IS NULL
	EXECUTE('CREATE PROCEDURE I_LoadAssigedToOfferInvestors AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_LoadAssigedToOfferInvestors
@CashRequestID BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		cr.ManagerApprovedSum, 
		o.InvestorID, 
		o.InvestmentPercent, 
		ibFunding.InvestorBankAccountID
	FROM 
		CashRequests cr
	INNER JOIN 
		I_OpenPlatformOffer o ON o.CashRequestID = cr.Id
	INNER JOIN 
		I_InvestorBankAccount ibFunding 
		ON 
			o.InvestorID=ibFunding.InvestorID 
		AND 
			ibFunding.InvestorAccountTypeID=1 
		AND 
			ibFunding.IsActive=1
	WHERE 
		cr.Id = @CashRequestID
END 
GO
