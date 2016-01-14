IF OBJECT_ID('I_InvestorBankAccountTransactionAdd') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorBankAccountTransactionAdd AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_InvestorBankAccountTransactionAdd
	@BankAccountID INT,
	@Date DATETIME,
	@TransactionAmount DECIMAL(18,6),
	@UserID INT,
	@Comment NVARCHAR(500)
AS
BEGIN
	SET NOCOUNT ON;


	DECLARE @PreviousBalance DECIMAL(18,6) = (SELECT 
												TOP 1 isnull(NewBalance, 0) 
											  FROM 
											  	I_InvestorBankAccountTransaction 
											  WHERE 
											  	InvestorBankAccountID=@BankAccountID 
											  ORDER BY 
											  	Timestamp DESC)
	IF(@PreviousBalance IS NULL)
		SET @PreviousBalance = 0
		

	INSERT INTO I_InvestorBankAccountTransaction (
		Timestamp,
		TransactionAmount,
		NewBalance,
		PreviousBalance,
		InvestorBankAccountID,
		UserID,
		Comment
	)
	VALUES (
		@Date, 
		@TransactionAmount, 
		@PreviousBalance + @TransactionAmount, 
		@PreviousBalance, 
		@BankAccountID, 
		@UserID,
		@Comment
	)
	
	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


