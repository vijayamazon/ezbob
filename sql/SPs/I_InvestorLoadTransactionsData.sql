IF OBJECT_ID('[I_InvestorLoadTransactionsData]') IS NULL
	EXECUTE('CREATE PROCEDURE [I_InvestorLoadTransactionsData] AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [I_InvestorLoadTransactionsData]
	@InvestorID INT,
	@BankAccountTypeID INT
AS

BEGIN
	SET NOCOUNT ON

	SELECT 
		ibat.InvestorBankAccountTransactionID AS TransactionID,
		ibat.Timestamp AS TransactionDate,
		ibat.TransactionAmount AS TransactionAmount,
		ibat.PreviousBalance AS PreviousAmount,
		ibat.NewBalance AS NewAmount,			
		iba.BankAccountNumber AS BankAccountNumber,
		iba.BankAccountName AS BankAccountName,		
		iba.IsActive AS IsBankAccountActive,
		ibat.Comment AS Comment
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
