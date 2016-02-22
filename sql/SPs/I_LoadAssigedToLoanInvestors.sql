SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_LoadAssigedToLoanInvestors') IS NULL
	EXECUTE('CREATE PROCEDURE I_LoadAssigedToLoanInvestors AS SELECT 1')
GO

ALTER PROCEDURE I_LoadAssigedToLoanInvestors
@LoanID INT
AS
BEGIN
	SELECT 
	   ibaRepayments.InvestorBankAccountID AS RepaymentBankAccountID,
	   ibaFunding.InvestorBankAccountID AS FundingBankAccountID,
	   o.InvestmentPercent AS InvestmentPercent,
	   o.InvestorID AS InvestorID,
	   l.CustomerSelectedTerm AS RepaymentPeriod,
	   l.LoanAmount AS LoanAmount,
	   i.DiscountServicingFeePercent AS DiscountServicingFeePercent,
	   pst.ProductTypeID AS ProductTypeID
	FROM 
		Loan l 
	INNER JOIN 
		CashRequests cr ON l.RequestCashId = cr.Id 
	INNER JOIN 
		I_OpenPlatformOffer o ON o.CashRequestID = cr.Id
	INNER JOIN 
		I_Investor i ON i.InvestorID = o.InvestorID	
	LEFT JOIN 
		I_InvestorBankAccount ibaFunding ON ibaFunding.InvestorID = o.InvestorID AND ibaFunding.InvestorAccountTypeID = 1 AND ibaFunding.IsActive=1	
	LEFT JOIN 
		I_InvestorBankAccount ibaRepayments ON ibaRepayments.InvestorID = o.InvestorID AND ibaRepayments.InvestorAccountTypeID = 2 AND ibaRepayments.IsActive=1	
	LEFT JOIN 
		I_ProductSubType pst ON cr.ProductSubTypeID = pst.ProductSubTypeID
	WHERE 
		l.Id = @LoanID  
		
END
GO
