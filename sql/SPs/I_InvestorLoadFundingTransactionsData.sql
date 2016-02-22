IF OBJECT_ID('[I_InvestorLoadFundingTransactionsData]') IS NULL
	EXECUTE('CREATE PROCEDURE [I_InvestorLoadFundingTransactionsData] AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_InvestorLoadFundingTransactionsData
	@InvestorID INT,
	@BankAccountTypeID INT
AS

BEGIN
	SET NOCOUNT ON

	SELECT 
		ibat.InvestorBankAccountTransactionID AS TransactionID,
		ibat.TransactionDate AS TransactionDate,
		ibat.TransactionAmount AS TransactionAmount,
		ibat.PreviousBalance AS PreviousAmount,
		ibat.NewBalance AS NewAmount,
		ibat.BankTransactionRef AS BankTransactionRef,			
		iba.BankAccountNumber AS BankAccountNumber,
		iba.BankAccountName AS BankAccountName,		
		iba.IsActive AS IsBankAccountActive,
		ibat.Comment AS Comment,
		ibat.Timestamp AS Timestamp
	FROM
		I_InvestorBankAccountTransaction ibat
	LEFT JOIN 
		I_InvestorBankAccount iba ON iba.InvestorBankAccountID = ibat.InvestorBankAccountID AND iba.InvestorAccountTypeID = @BankAccountTypeID
	LEFT JOIN 
		I_Investor i ON i.InvestorID = iba.InvestorID	
	WHERE 
		i.InvestorID = @InvestorID
	ORDER BY
		ibat.Timestamp 	DESC
END
GO
