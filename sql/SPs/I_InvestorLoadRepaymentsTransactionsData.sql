IF OBJECT_ID('[I_InvestorLoadRepaymentsTransactionsData]') IS NULL
	EXECUTE('CREATE PROCEDURE [I_InvestorLoadRepaymentsTransactionsData] AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [I_InvestorLoadRepaymentsTransactionsData]
	@InvestorID INT,
	@BankAccountTypeID INT
AS

BEGIN
	SET NOCOUNT ON

	SELECT 
		isb.InvestorSystemBalanceID AS TransactionID,
		isb.TransactionDate AS TransactionDate,
		isb.TransactionAmount AS TransactionAmount,
		isb.PreviousBalance AS PreviousAmount,
		isb.NewBalance AS NewAmount,
		'' AS BankTransactionRef,			
		iba.BankAccountNumber AS BankAccountNumber,
		iba.BankAccountName AS BankAccountName,		
		iba.IsActive AS IsBankAccountActive,
		isb.Comment AS Comment,
		isb.UserID ,
		isb.Timestamp AS Timestamp,
		isb.[NLOfferID] , 
		isb.[NLLoanID],
		isb.[NLPaymentID]
	FROM
		I_InvestorSystemBalance isb
	LEFT JOIN 
		I_InvestorBankAccount iba ON iba.InvestorBankAccountID = isb.InvestorBankAccountID AND iba.InvestorAccountTypeID = @BankAccountTypeID
	LEFT JOIN 
		I_Investor i ON i.InvestorID = iba.InvestorID	
	WHERE 
		i.InvestorID = @InvestorID AND isb.NewBalance > isb.PreviousBalance
END
GO
